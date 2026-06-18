# Spirantis.Extensions.Threading

A small set of asynchronous coordination primitives for .NET that fill gaps in the
built-in BCL types — FIFO-ordered async locking and serial execution of async work,
both available globally or partitioned per key.

- **`FifoSemaphore`** — an async semaphore that grants entry in strict first-in,
  first-out order (`SemaphoreSlim` does not guarantee ordering).
- **`FunctionExecutionQueue`** — runs async actions one at a time, in enqueue order.
- **`KeyedFifoSemaphore`** — per-key FIFO mutual exclusion; locks on the same key
  serialize, different keys run concurrently.
- **`KeyedFunctionExecutionQueue<TKey>`** — a `FunctionExecutionQueue` per key.

Targets **.NET 10**. MIT licensed.

## Installation

```bash
dotnet add package Spirantis.Extensions.Threading
```

## Usage

### FifoSemaphore

Like `SemaphoreSlim`, but waiters are released in the exact order they called
`WaitAsync`. You release a slot manually with `Release`.

```csharp
using Spirantis.Extensions.Threading;

using var semaphore = new FifoSemaphore(initialCount: 1);

await semaphore.WaitAsync();
try
{
    // critical section — only one caller here at a time, granted in FIFO order
}
finally
{
    semaphore.Release();
}
```

The two-argument constructor sets a maximum count:

```csharp
using var semaphore = new FifoSemaphore(initialCount: 2, maxCount: 2);
```

### FunctionExecutionQueue

Enqueue async actions and they run strictly one after another, in order — the next
one starts only once the previous has fully completed. `Enqueue` returns
immediately; the work runs on the thread pool.

```csharp
var queue = new FunctionExecutionQueue();

queue.OnActionExecuted += () => Console.WriteLine("an action finished");
queue.OnLastAction += () => Console.WriteLine("queue drained");

queue.Enqueue(async () => await SaveAsync(first));
queue.Enqueue(async () => await SaveAsync(second)); // runs after the first completes
```

A failing action does not stall the queue or skip the following actions.

### KeyedFifoSemaphore

Per-key FIFO mutual exclusion. `WaitAsync` returns an `IDisposable`; dispose it to
release. The underlying semaphore for a key is created on first use and removed
once its last holder releases it.

```csharp
var locks = new KeyedFifoSemaphore();

locks.LastProcessForKey += (_, key) => Console.WriteLine($"'{key}' fully released");

// Calls with the same key serialize; different keys run concurrently.
using (await locks.WaitAsync("order-123"))
{
    await ProcessAsync("order-123");
}

bool busy = locks.ContainsKey("order-123");
bool idle = locks.IsEmpty;
```

Disposing the returned handle more than once is safe.

### KeyedFunctionExecutionQueue&lt;TKey&gt;

Serial execution **per key**: actions sharing a key run one at a time in order,
while actions under different keys run concurrently. A key's queue is removed once
it drains.

```csharp
var queue = new KeyedFunctionExecutionQueue<string>();

queue.OnActionExecuted += key => Console.WriteLine($"action for '{key}' finished");
queue.OnLastAction += key => Console.WriteLine($"'{key}' drained");

queue.Enqueue("user-1", async () => await HandleAsync(a)); // serialized with...
queue.Enqueue("user-1", async () => await HandleAsync(b)); // ...this one
queue.Enqueue("user-2", async () => await HandleAsync(c)); // runs concurrently
```

## License

[MIT](LICENSE)
