import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ConfigService } from '../services/config.service';
import { Subscription } from 'rxjs';
import { ChatService, DialogflowResponse } from '../services/chat.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface ChatMessage {
  text: string;
  isUser: boolean;
  timestamp: Date;
  branch?: string;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit, OnDestroy {
  bots: any[] = [];
  selectedBot: any | null = null;
  showBotList = false;
  userMessage = '';
  isConnecting = false;
  isProcessingMessages = false;

  chatHistories: Record<string, ChatMessage[]> = {};

  private messageSubscription: Subscription | null = null;
  private configSubscription: Subscription | null = null;
  private silenceTimer: any = null;
  private readonly silenceTimeout = 35000;

  private boundResetSilenceTimer: () => void = () => { };
  private trackedBotId: string = '';

  constructor(
    private configService: ConfigService,
    private chatService: ChatService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.initializeChat();
  }

  get messages() {
    return this.selectedBot?.Id ? this.chatHistories[this.selectedBot.Id] || [] : [];
  }

  initializeChat() {
    const token = localStorage.getItem('auth_token');
    if (!token) return;

    this.isConnecting = true;
    this.configSubscription = this.configService.getConfigs().subscribe({
      next: (bots) => {
        this.bots = bots;
        if (bots.length) {
          this.selectedBot = bots[0];
          this.initializeBotChat(bots[0].Id, token);
        }
        this.isConnecting = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to fetch bots:', err);
        this.isConnecting = false;
        this.cdr.detectChanges();
      }
    });
  }

  initializeBotChat(botId: string, token: string) {
    if (!this.chatHistories[botId]) {
      this.chatHistories[botId] = [];
      console.log('Initialized new chat history for bot:', botId);
    }

    this.chatService.close();
    this.messageSubscription?.unsubscribe();
    this.trackSilence(botId); 

    console.log('Subscribing to WebSocket messages for bot:', botId);
    this.messageSubscription = this.chatService.connect(token).subscribe({
      next: (response: DialogflowResponse) => {
        console.log('Received Dialogflow response:', response);
        if (this.selectedBot?.Id === botId) {
          console.log('Processing response for current bot');
          this.processDialogflowResponse(response, botId);
        } else {
          console.log('Ignoring response for non-selected bot');
        }
      },
      error: (err) => {
        console.error('WebSocket error:', err);
        this.cdr.detectChanges();
      },
      complete: () => {
        console.log('WebSocket subscription completed');
      }
    });
  }

  private processDialogflowResponse(response: DialogflowResponse, botId: string) {
    if (!response) {
      console.error('Received undefined response');
      return;
    }

    if (!this.chatHistories[botId]) {
      this.chatHistories[botId] = [];
    }

    const messages = (response.messages || []).filter(
      msg => msg.trim().toLowerCase() !== 'script payload'
    );

    if (messages.length > 1) {
      messages.forEach((message, index) => {
        setTimeout(() => {
          this.addMessageToHistory({
            text: message,
            isUser: false,
            timestamp: new Date(),
            branch: response.resultBranch
          }, botId);
        }, index * 800);
      });
    } else {
      const messageToShow = messages[0] || response.fulfillmentText || '';
      this.addMessageToHistory({
        text: messageToShow,
        isUser: false,
        timestamp: new Date(),
        branch: response.resultBranch
      }, botId);
    }
  }

  private resetSilenceTimer(botId: string) {
    if (this.silenceTimer) {
      clearTimeout(this.silenceTimer);
    }

    this.silenceTimer = setTimeout(() => {
      this.chatService.sendMessage('SILENCE');
    }, this.silenceTimeout);
  }

  private trackSilence(botId: string) {
    this.trackedBotId = botId;

    this.removeSilenceListeners();

    this.boundResetSilenceTimer = () => this.resetSilenceTimer(botId);
    this.resetSilenceTimer(botId);

    document.addEventListener('keydown', this.boundResetSilenceTimer);
    document.addEventListener('click', this.boundResetSilenceTimer);
    document.addEventListener('mousemove', this.boundResetSilenceTimer);
    document.addEventListener('focusin', this.boundResetSilenceTimer);

    const inputEl = document.getElementById('userInput');
    if (inputEl) {
      inputEl.addEventListener('input', this.boundResetSilenceTimer);
    }
  }

  private removeSilenceListeners() {
    if (this.boundResetSilenceTimer) {
      document.removeEventListener('keydown', this.boundResetSilenceTimer);
      document.removeEventListener('click', this.boundResetSilenceTimer);
      document.removeEventListener('mousemove', this.boundResetSilenceTimer);
      document.removeEventListener('focusin', this.boundResetSilenceTimer);

      const inputEl = document.getElementById('userInput');
      if (inputEl) {
        inputEl.removeEventListener('input', this.boundResetSilenceTimer);
      }
    }
  }

  private addMessageToHistory(message: ChatMessage, botId: string) {
    if (!this.chatHistories[botId]) {
      this.chatHistories[botId] = [];
    }
    this.chatHistories[botId].push(message);
    this.cdr.detectChanges();
  }

  sendMessage() {
    if (!this.userMessage.trim() || !this.selectedBot || this.isProcessingMessages) return;

    const botId = this.selectedBot.Id;
    this.addMessageToHistory({
      text: this.userMessage,
      isUser: true,
      timestamp: new Date()
    }, botId);

    this.chatService.sendMessage(this.userMessage);
    this.userMessage = '';

    this.resetSilenceTimer(botId);
  }

  selectBot(bot: any) {
    const token = localStorage.getItem('auth_token');
    if (!token) return;

    this.isConnecting = true;
    this.selectedBot = bot;
    this.showBotList = false;
    this.cdr.detectChanges();

    setTimeout(() => {
      this.initializeBotChat(bot.Id, token);
      this.isConnecting = false;
      this.cdr.detectChanges();
    }, 0);
  }

  toggleBotList() {
    this.showBotList = !this.showBotList;
    this.cdr.detectChanges();
  }

  ngOnDestroy() {
    this.messageSubscription?.unsubscribe();
    this.configSubscription?.unsubscribe();
    this.chatService.close();
    if (this.silenceTimer) clearTimeout(this.silenceTimer);

    this.removeSilenceListeners();
  }
}
