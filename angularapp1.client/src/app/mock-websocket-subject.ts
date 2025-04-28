// mock-websocket-subject.ts
import { Subject } from 'rxjs';

export class MockWebSocketSubject<T> extends Subject<T> {
  override closed = false;
  nextCalledWith: T[] = [];

  override next(value: T): void {
    this.nextCalledWith.push(value);
    super.next(value);
  }

  override complete(): void {
    this.closed = true;
    super.complete();
  }

  override error(err: any): void {
    super.error(err);
  }
}
