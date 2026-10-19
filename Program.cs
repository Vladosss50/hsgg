using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzeriaOOP
{
    public abstract class MenuItem
    {
        private string _name;
        protected decimal _basePrice;

        public string Name
        {
            get => _name;
        }

        public MenuItem(string name, decimal basePrice)
        {
            if (basePrice < 0)
                throw new ArgumentException("Базовая цена не может быть отрицательной.");

            _name = name;
            _basePrice = basePrice;
        }

        public abstract decimal GetFinalPrice();
        public abstract string GetInfo();
        public virtual string GetCategory() => "Позиция меню";
    }

    public interface IDiscountable
    {
        decimal ApplyDiscount(decimal percent);
        string GetDiscountInfo();
    }

    public class Pizza : MenuItem, IDiscountable
    {
        public int ToppingsCount { get; }

        public Pizza(string name, decimal basePrice, int toppingsCount) 
            : base(name, basePrice)
        {
            ToppingsCount = toppingsCount;
        }

        public override decimal GetFinalPrice()
        {
            return _basePrice + ToppingsCount * 50;
        }

        public override string GetCategory() => "Пицца";

        public override string GetInfo()
        {
            return $"{GetCategory()} «{Name}», топпингов: {ToppingsCount}, итого: {GetFinalPrice():F2} руб.";
        }

        public decimal ApplyDiscount(decimal percent)
        {
            decimal currentPrice = GetFinalPrice();
            if (ToppingsCount > 3)
            {
                currentPrice *= 0.95m;
            }
            return currentPrice * (1 - percent / 100);
        }

        public string GetDiscountInfo()
        {
            return ToppingsCount > 3 ? " (Включая доп. скидку 5% за >3 топпингов)" : "";
        }
    }

    public class Drink : MenuItem
    {
        public int VolumeMl { get; }

        public Drink(string name, decimal basePrice, int volumeMl) 
            : base(name, basePrice)
        {
            VolumeMl = volumeMl;
        }

        public override decimal GetFinalPrice()
        {
            return _basePrice + VolumeMl * 0.5m;
        }

        public override string GetCategory() => "Напиток";

        public override string GetInfo()
        {
            return $"{GetCategory()} «{Name}», {VolumeMl} мл, итого: {GetFinalPrice():F2} руб.";
        }
    }

    public class Dessert : MenuItem, IDiscountable
    {
        public bool IsTodayFresh { get; }

        public Dessert(string name, decimal basePrice, bool isTodayFresh) 
            : base(name, basePrice)
        {
            IsTodayFresh = isTodayFresh;
        }

        public override decimal GetFinalPrice()
        {
            return IsTodayFresh ? _basePrice : _basePrice * 0.7m;
        }

        public override string GetCategory() => "Десерт";

        public override string GetInfo()
        {
            string status = IsTodayFresh ? "свежий" : "вчерашний (распродажа -30%)";
            return $"{GetCategory()} «{Name}», {status}, итого: {GetFinalPrice():F2} руб.";
        }

        public decimal ApplyDiscount(decimal percent)
        {
            decimal currentPrice = GetFinalPrice();
            if (!IsTodayFresh)
            {
                currentPrice *= 0.90m;
            }
            return currentPrice * (1 - percent / 100);
        }

        public string GetDiscountInfo()
        {
            return !IsTodayFresh ? " (Включая доп. скидку 10% за распродажу)" : "";
        }
    }

    public class Combo : MenuItem
    {
        private readonly List<MenuItem> _comboItems = new();

        public Combo(string name, List<MenuItem> items) : base(name, 0)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Комбо-набор не может быть пустым.");
            
            _comboItems.AddRange(items);
        }

        public override decimal GetFinalPrice()
        {
            decimal total = _comboItems.Sum(item => item.GetFinalPrice());
            return total * 0.85m;
        }

        public override string GetCategory() => "Комбо-набор";

        public override string GetInfo()
        {
            string itemsList = string.Join(", ", _comboItems.Select(i => i.Name));
            return $"{GetCategory()} «{Name}» [{itemsList}], итого (со скидкой 15%): {GetFinalPrice():F2} руб.";
        }
    }

    public class Order
    {
        private readonly List<MenuItem> _items = new();
        private const int MaxItems = 20;

        public void Add(MenuItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Позиция не может быть null.");

            if (_items.Count >= MaxItems)
                throw new InvalidOperationException($"Нельзя добавить больше {MaxItems} позиций in один заказ.");

            _items.Add(item);
        }

        public IReadOnlyList<MenuItem> GetItems()
        {
            return _items.AsReadOnly();
        }

        public decimal GetTotal()
        {
            return _items.Sum(item => item.GetFinalPrice());
        }

        public void PrintReceipt()
        {
            Console.WriteLine("=== ЧЕК ===");
            foreach (var item in _items)
            {
                Console.WriteLine($"[{item.GetCategory()}] {item.GetInfo()}");
            }
            Console.WriteLine(new string('-', 40));
            Console.WriteLine($"Итого: {GetTotal():F2} руб.");
        }

        public decimal GetTotalWithDiscount(decimal percent)
        {
            decimal total = 0;
            foreach (var item in _items)
            {
                if (item is IDiscountable discountableItem)
                {
                    total += discountableItem.ApplyDiscount(percent);
                }
                else
                {
                    total += item.GetFinalPrice();
                }
            }
            return total;
        }

        public MenuItem FindMostExpensive()
        {
            if (_items.Count == 0) return null;
            
            return _items.OrderByDescending(item => item.GetFinalPrice()).First();
        }

        public void PrintSortedByPrice()
        {
            Console.WriteLine("\n=== ПОЗИЦИИ, СОРТИРОВАННЫЕ ПО ЦЕНЕ ===");
            var sorted = _items.OrderBy(item => item.GetFinalPrice());
            foreach (var item in sorted)
            {
                Console.WriteLine($"{item.Name}: {item.GetFinalPrice():F2} руб.");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Order order = new Order();

            Pizza margarita = new Pizza("Маргарита", 400, 3);
            Pizza pepperoni = new Pizza("Пепперони Мясная", 450, 5);
            Drink cola = new Drink("Кола", 50, 500);
            Drink juice = new Drink("Апельсиновый сок", 70, 300);
            Dessert tiramisu = new Dessert("Тирамису", 320, true);
            Dessert cheesecake = new Dessert("Чизкейк", 280, false);

            order.Add(margarita);
            order.Add(pepperoni);
            order.Add(cola);
            order.Add(juice);
            order.Add(tiramisu);
            order.Add(cheesecake);

            var comboItems = new List<MenuItem> { new Pizza("Мини-Пицца", 200, 1), new Drink("Вода", 30, 500) };
            Combo lunchCombo = new Combo("Экспресс Ланч", comboItems);
            order.Add(lunchCombo);

            order.PrintReceipt();

            decimal discountPercent = 10;
            decimal totalWithDiscount = order.GetTotalWithDiscount(discountPercent);
            Console.WriteLine($"Скидка {discountPercent}%: {totalWithDiscount:F2} руб. (с учетом правил категорий)");

            var expensive = order.FindMostExpensive();
            if (expensive != null)
            {
                Console.WriteLine($"\nСамая дорогая позиция: {expensive.Name} ({expensive.GetFinalPrice():F2} руб.)");
            }
            order.PrintSortedByPrice();

            IReadOnlyList<MenuItem> externalList = order.GetItems();
            
            // externalList.Add(new Drink("Контрабандный напиток", 100, 500)); 

            Console.WriteLine("\n[Защита инкапсуляции]: Изменение списка через GetItems() заблокировано на уровне компиляции.");
        }
    }
}
