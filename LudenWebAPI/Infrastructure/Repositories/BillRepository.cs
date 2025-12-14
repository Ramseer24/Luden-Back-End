using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.FirebaseDatabase;

namespace Infrastructure.Repositories
{
    public class BillRepository : GenericRepository<Bill>, IBillRepository
    {
        private readonly FirebaseRepository _firebaseRepo;

        public BillRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo)
        {
            _firebaseRepo = firebaseRepo;
        }

        public async Task<IEnumerable<Bill>> GetBillsByUserIdAsync(ulong userId)
        {
            var bills = new List<Bill>();

            // Firebase не поддерживает сложные запросы, поэтому фильтруем вручную
            await _firebaseRepo.GetAsync<Dictionary<string, Bill>>(
                "bills",
                new FirebaseConsoleListener<Dictionary<string, Bill>>(data =>
                {
                    if (data != null)
                    {
                        foreach (var b in data.Values)
                        {
                            if (b.UserId == userId)
                                bills.Add(b);
                        }
                    }
                })
            );

            return bills.OrderByDescending(b => b.CreatedAt);
        }
    }

    // 🔹 Утилитарный listener для вывода в консоль (общий)
    public class FirebaseConsoleListener<T> : IFirebaseListener
    {
        private readonly Action<T> _onData;

        public FirebaseConsoleListener(Action<T> onData)
        {
            _onData = onData;
        }

        public void OnSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[Firebase OK] {message}");
            Console.ResetColor();
        }

        public void OnError(string reason)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Firebase ERROR] {reason}");
            Console.ResetColor();
        }

        public void OnDataSnapshot<TData>(TData data)
        {
            if (data is T casted)
                _onData(casted);
        }
    }
}
