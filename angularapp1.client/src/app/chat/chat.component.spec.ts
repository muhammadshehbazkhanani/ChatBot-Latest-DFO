import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ChatComponent } from './chat.component';
import { ConfigService } from '../services/config.service';
import { ChatService, DialogflowResponse } from '../services/chat.service';
import { of, Subject, Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

describe('ChatComponent (standalone)', () => {
  let fixture: ComponentFixture<ChatComponent>;
  let component: ChatComponent;
  let configServiceSpy: jasmine.SpyObj<ConfigService>;
  let chatServiceSpy: jasmine.SpyObj<ChatService>;
  let wsSubject: Subject<DialogflowResponse>;

  beforeEach(async () => {
    configServiceSpy = jasmine.createSpyObj('ConfigService', ['getConfigs']);
    chatServiceSpy = jasmine.createSpyObj('ChatService', ['connect', 'close', 'sendMessage']);

    wsSubject = new Subject<DialogflowResponse>();
    chatServiceSpy.connect.and.returnValue(wsSubject.asObservable());

    await TestBed.configureTestingModule({
      imports: [CommonModule, FormsModule, ChatComponent],
      providers: [
        { provide: ConfigService, useValue: configServiceSpy },
        { provide: ChatService, useValue: chatServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ChatComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    wsSubject.complete();
    localStorage.removeItem('auth_token');
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should fetch configs and initialize chat on ngOnInit', fakeAsync(() => {
    // Arrange: set auth token so initialization proceeds
    localStorage.setItem('auth_token', 'tokenABC');
    const bots = [{ Id: 'bot1' }, { Id: 'bot2' }];
    configServiceSpy.getConfigs.and.returnValue(of(bots));
    spyOn(component, 'initializeBotChat');

    // Act: trigger Angular lifecycle
    fixture.detectChanges(); // calls ngOnInit internally
    tick();

    // Assert
    expect(configServiceSpy.getConfigs).toHaveBeenCalled();
    expect(component.bots).toEqual(bots);
    expect(component.selectedBot).toEqual(bots[0]);
    const token = localStorage.getItem('auth_token')!;
    expect(component.initializeBotChat).toHaveBeenCalledWith('bot1', token);
    expect(component.isConnecting).toBeFalse();
  }));

  it('should process single message from DialogflowResponse', fakeAsync(() => {
    component.chatHistories['bot1'] = [];
    component.selectedBot = { Id: 'bot1' } as any;

    const response: DialogflowResponse = { messages: ['Hi'], fulfillmentText: '', resultBranch: 'branchA' } as any;
    component['processDialogflowResponse'](response, 'bot1');

    expect(component.chatHistories['bot1'].length).toBe(1);
    expect(component.chatHistories['bot1'][0].text).toBe('Hi');
    expect(component.chatHistories['bot1'][0].branch).toBe('branchA');
  }));

  it('should process multiple messages sequentially', fakeAsync(() => {
    component.chatHistories['botX'] = [];
    component.selectedBot = { Id: 'botX' } as any;

    const response: DialogflowResponse = { messages: ['One', 'Two', 'Three'], fulfillmentText: '', resultBranch: 'b' } as any;
    component['processDialogflowResponse'](response, 'botX');
    tick(2500);

    expect(component.chatHistories['botX'].map(m => m.text)).toEqual(['One', 'Two', 'Three']);
  }));

  it('should send user message and clear input', () => {
    component.selectedBot = { Id: 'bot1' } as any;
    component.userMessage = 'Hello';
    component.chatHistories['bot1'] = [];

    component.sendMessage();

    const msgs = component.chatHistories['bot1'];
    expect(msgs[msgs.length - 1].text).toBe('Hello');
    expect(chatServiceSpy.sendMessage).toHaveBeenCalledWith('Hello');
    expect(component.userMessage).toBe('');
  });

  it('should toggle bot list visibility', () => {
    expect(component.showBotList).toBeFalse();
    component.toggleBotList();
    expect(component.showBotList).toBeTrue();
  });

  it('should select a bot and reinitialize chat', fakeAsync(() => {
    component.bots = [{ Id: 'A' }, { Id: 'B' }] as any;
    localStorage.setItem('auth_token', 'tok123');
    spyOn(component, 'initializeBotChat');

    component.selectBot(component.bots[1]);
    tick();

    expect(component.selectedBot).toEqual(component.bots[1]);
    expect(component.showBotList).toBeFalse();
    expect(component.initializeBotChat).toHaveBeenCalledWith('B', 'tok123');
  }));

  it('should clean up subscriptions and listeners on destroy', () => {
    // Prepare dummy subscriptions with real Subscription instances
    const msgSub = new Subscription();
    spyOn(msgSub, 'unsubscribe');
    component['messageSubscription'] = msgSub;

    const cfgSub = new Subscription();
    spyOn(cfgSub, 'unsubscribe');
    component['configSubscription'] = cfgSub;

    // chatServiceSpy.close is already a spy
    spyOn(window, 'clearTimeout');
    component['silenceTimer'] = 123;
    component['removeSilenceListeners'] = jasmine.createSpy('removeListeners');

    component.ngOnDestroy();

    expect(msgSub.unsubscribe).toHaveBeenCalled();
    expect(cfgSub.unsubscribe).toHaveBeenCalled();
    expect(chatServiceSpy.close).toHaveBeenCalled();
    expect(window.clearTimeout).toHaveBeenCalledWith(123);
    expect(component['removeSilenceListeners']).toHaveBeenCalled();
  });
});
