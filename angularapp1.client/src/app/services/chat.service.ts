// chat.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebSocketSubject, webSocket } from 'rxjs/webSocket';
import { Observable, Subject, throwError } from 'rxjs';
import { catchError, tap, map, filter } from 'rxjs/operators';

export interface DialogflowResponse {
  fulfillmentText: string;
  resultBranch?: string;
  intentName?: string;
  messages?: string[];
  isMultipleMessages?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private socket$!: WebSocketSubject<any>;
  private readonly baseUrl = 'ws://localhost:8080/ws';
  private messageSubject = new Subject<DialogflowResponse>();
  private messageBuffer: string[] = [];

  constructor(private http: HttpClient) { }

  sendCustomPayload(customPayload: any): void {
    if (!this.socket$ || this.socket$.closed) {
      throw new Error('WebSocket not connected');
    }

    // Send the custom payload to the Bot
    const payloadMessage = {
      customPayload: customPayload
    };

    this.socket$.next(payloadMessage);
  }


  connect(token: string): Observable<DialogflowResponse> {
    const encodedToken = encodeURIComponent(token);
    const wsUrl = `${this.baseUrl}?access_token=${encodedToken}`;

    console.info('Connecting to WebSocket at:', wsUrl);
    this.close();

    this.socket$ = webSocket({
      url: wsUrl,
      openObserver: {
        next: () => console.info('WebSocket connection established')
      },
      closeObserver: {
        next: () => console.warn('WebSocket connection closed')
      },
      deserializer: ({ data }) => {
        console.info('Raw WebSocket message received:', data);
        return data;
      },
      serializer: (msg: string) => msg
    });

    return this.socket$.pipe(
      tap(data => console.info('Before normalization:', data)),
      map(data => this.normalizeResponse(data)),
      tap(normalized => console.info('After normalization:', normalized)),
      filter(response => response !== null),
      catchError(error => {
        console.error('WebSocket error:', error);
        return throwError(() => error);
      })
    );
  }

  private normalizeResponse(data: any): DialogflowResponse | null {

    // Handle custom payload for StandardBotExchangeCustomInput
    if (data.customPayload) {
      // Extract and format custom payload
      const customPayload = data.customPayload;
      const customMessage = `Custom Payload: ${JSON.stringify(customPayload, null, 2)}`;

      return {
        fulfillmentText: customMessage,
        resultBranch: 'PromptAndCollectNextResponse',
        messages: [customMessage],
        isMultipleMessages: false
      };
    }

    if (typeof data === 'object' && data.fulfillmentText) {
      return {
        fulfillmentText: data.fulfillmentText,
        resultBranch: data.resultBranch || 'PromptAndCollectNextResponse',
        messages: Array.isArray(data.messages) ? data.messages : [],
        isMultipleMessages: Array.isArray(data.messages) && data.messages.length > 1
      };
    }

    if (typeof data === 'string') {
      try {
        const parsed = JSON.parse(data);
        const rawMsgs = parsed.messages ?? parsed.Messages;

        if (Array.isArray(rawMsgs)) {
          return {
            fulfillmentText: parsed.fulfillmentText ?? rawMsgs.join('\n'),
            resultBranch: parsed.resultBranch ?? 'PromptAndCollectNextResponse',
            messages: rawMsgs,
            isMultipleMessages: rawMsgs.length > 1
          };
        }

        return {
          fulfillmentText: parsed.fulfillmentText ?? parsed.text ?? data,
          resultBranch: parsed.resultBranch ?? 'PromptAndCollectNextResponse',
          messages: [parsed.fulfillmentText ?? parsed.text ?? data],
          isMultipleMessages: false
        };

      } catch (e) {
        return {
          fulfillmentText: data,
          resultBranch: this.determineBranchFromText(data),
          messages: [data],
          isMultipleMessages: false
        };
      }
    }

    return {
      fulfillmentText: JSON.stringify(data),
      resultBranch: 'PromptAndCollectNextResponse',
      messages: [JSON.stringify(data)],
      isMultipleMessages: false
    };
  }


  //private determineBranch(queryResult: any): string {
  //  if (!queryResult) return 'PromptAndCollectNextResponse';

  //  const isEndConversation =
  //    queryResult.intent?.displayName === 'StandardBotEndConversation' ||
  //    queryResult.diagnosticInfo?.fields?.end_conversation?.boolValue === true ||
  //    queryResult.endInteraction === true;

  //  return isEndConversation ? 'ReturnControlToScript' : 'PromptAndCollectNextResponse';
  //}

  private determineBranchFromText(text: string): string {
    return text.toLowerCase().includes('bye')
      ? 'ReturnControlToScript'
      : 'PromptAndCollectNextResponse';
  }

  //private extractMessages(messages: any[]): string[] {
  //  if (!Array.isArray(messages)) return [];
  //  return messages
  //    .filter(msg => msg?.text?.text?.length)
  //    .flatMap(msg => msg.text.text)
  //    .filter(text => typeof text === 'string');
  //}

  sendMessage(message: string): void {
    if (!this.socket$ || this.socket$.closed) {
      throw new Error('WebSocket not connected');
    }
    this.socket$.next(message);
  }

  close(): void {
    this.socket$?.complete();
    this.messageSubject.complete();
    this.messageBuffer = [];
  }
}
