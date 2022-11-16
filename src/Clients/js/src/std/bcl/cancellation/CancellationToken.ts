import { EmptyCancellationToken, CancellationTokenRegistration } from '.';
import { PromiseCompletionSource, PromisePal } from '../promises';

export abstract class CancellationToken {
    public static get none(): CancellationToken { return EmptyCancellationToken.instance; }

    protected constructor() { }

    public abstract get canBeCanceled(): boolean;
    public abstract get isCancellationRequested(): boolean;
    public abstract throwIfCancellationRequested(): void | never;
    public abstract register(callback: () => void): CancellationTokenRegistration;

    public bind<T>(pcs: PromiseCompletionSource<T>): void {
        if (this.isCancellationRequested) { pcs.trySetCanceled(); }
        if (!this.canBeCanceled) { return; }

        const reg = this.register(() => pcs.trySetCanceled());
        const _ = PromisePal.ensureObserved((async () => {
            try {
                await pcs.promise;
            } finally {
                reg.dispose();
            }
        })());
    }
}
