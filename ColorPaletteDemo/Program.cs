using System;
using System.Collections.Generic;

namespace ColorPaletteDemo
{
    // 1. ТИП ARGBCOLOR
    public class ArgbColor
    {
        public byte A { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public ArgbColor(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public byte this[int channel]
        {
            get
            {
                if (channel == 0) return A;
                if (channel == 1) return R;
                if (channel == 2) return G;
                if (channel == 3) return B;
                throw new ArgumentOutOfRangeException("Индекс должен быть от 0 до 3");
            }
        }

        public bool IsGray
        {
            get { return R == G && G == B; }
        }

        public uint ToRgb32()
        {
            uint result = 0;
            result = result | ((uint)A << 24);
            result = result | ((uint)R << 16);
            result = result | ((uint)G << 8);
            result = result | B;
            return result;
        }

        public static ArgbColor FromRgb32(uint argb)
        {
            byte a = (byte)(argb >> 24);
            byte r = (byte)(argb >> 16);
            byte g = (byte)(argb >> 8);
            byte b = (byte)argb;
            return new ArgbColor(a, r, g, b);
        }

        public static implicit operator uint(ArgbColor color)
        {
            return color.ToRgb32();
        }

        public static implicit operator ArgbColor(uint argb)
        {
            return FromRgb32(argb);
        }

        public static ArgbColor Blend(in ArgbColor fg, in ArgbColor bg)
        {
            double af = fg.A / 255.0;
            double ab = bg.A / 255.0;
            double ao = af + ab * (1.0 - af);

            if (ao <= 0.0) return new ArgbColor(0, 0, 0, 0);

            byte newR = MixChannel(fg.R, bg.R, af, ab, ao);
            byte newG = MixChannel(fg.G, bg.G, af, ab, ao);
            byte newB = MixChannel(fg.B, bg.B, af, ab, ao);
            byte newA = (byte)Math.Round(ao * 255.0);

            return new ArgbColor(newA, newR, newG, newB);
        }

        private static byte MixChannel(byte cf, byte cb, double af, double ab, double ao)
        {
            double value = (cf * af + cb * ab * (1.0 - af)) / ao;
            int rounded = (int)Math.Round(value);
            if (rounded < 0) rounded = 0;
            if (rounded > 255) rounded = 255;
            return (byte)rounded;
        }

        public override string ToString()
        {
            return "#" + A.ToString("X2") + R.ToString("X2") + G.ToString("X2") + B.ToString("X2");
        }
    }

    // 2. КЛАСС PALETTE
    public class Palette
    {
        private Dictionary<string, ArgbColor> colors = new Dictionary<string, ArgbColor>();

        public void Add(string name, ArgbColor color)
        {
            if (name == null || name.Trim() == "")
                throw new ArgumentException("Имя не может быть пустым");

            colors[name] = color;
        }

        public bool Remove(string name)
        {
            if (name == null) return false;
            return colors.Remove(name);
        }

        public ArgbColor this[string name]
        {
            get
            {
                if (name == null) return null;
                if (colors.ContainsKey(name)) return colors[name];
                return null;
            }
        }

        public IReadOnlyCollection<(string name, ArgbColor color)> GetByPrefix(string prefix)
        {
            if (prefix == null) prefix = "";

            List<(string name, ArgbColor color)> result = new List<(string name, ArgbColor color)>();

            foreach (var pair in colors)
            {
                if (pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add((pair.Key, pair.Value));
                }
            }
            return result;
        }

        // Новый метод для вывода всех цветов
        public IReadOnlyCollection<(string name, ArgbColor color)> GetAll()
        {
            List<(string name, ArgbColor color)> result = new List<(string name, ArgbColor color)>();
            foreach (var pair in colors)
            {
                result.Add((pair.Key, pair.Value));
            }
            return result;
        }
    }

    // 3. ГЛАВНАЯ ПРОГРАММА С МЕНЮ
    internal class Program
    {
        private static void Main()
        {
            Palette myPalette = new Palette();
            
            // Добавим пару цветов для демонстрации
            myPalette.Add("Красный", new ArgbColor(255, 255, 0, 0));
            myPalette.Add("Зеленый", new ArgbColor(255, 0, 255, 0));

            while (true)
            {
                try { Console.Clear(); } catch { Console.WriteLine(); }

                Console.WriteLine("========================================");
                Console.WriteLine("           ЦВЕТОВАЯ ПАЛИТРА");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Показать все цвета");
                Console.WriteLine("2. Тест цветов (ArgbColor)");
                Console.WriteLine("3. Добавить цвет в палитру");
                Console.WriteLine("4. Найти цвет по имени");
                Console.WriteLine("5. Найти цвета по началу имени");
                Console.WriteLine("6. Удалить цвет");
                Console.WriteLine("7. Тест обработки ошибок");
                Console.WriteLine("0. Выход");
                Console.WriteLine("========================================");
                Console.Write("Выберите пункт: ");

                string choice = Console.ReadLine();
                if (choice == "0") break;
                Console.WriteLine("----------------------------------------");

                if (choice == "1") ShowAllColors(myPalette);
                else if (choice == "2") TestColors();
                else if (choice == "3") AddColor(myPalette);
                else if (choice == "4") FindColor(myPalette);
                else if (choice == "5") FindPrefix(myPalette);
                else if (choice == "6") DeleteColor(myPalette);
                else if (choice == "7") TestErrors();
                else Console.WriteLine("ОШИБКА: Такого пункта нет. Введите цифру от 0 до 7.");

                Console.WriteLine("\nНажмите Enter, чтобы вернуться в меню...");
                Console.ReadLine();
            }
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ МЕНЮ ---

        private static void ShowAllColors(Palette palette)
        {
            var all = palette.GetAll();
            if (all.Count == 0)
            {
                Console.WriteLine("Палитра пуста.");
                return;
            }

            Console.WriteLine("Цвета в палитре (" + all.Count + "):");
            foreach (var item in all)
            {
                Console.WriteLine(" - " + item.name + " : " + item.color);
            }
        }

        private static void TestColors()
        {
            ArgbColor red = new ArgbColor(255, 255, 0, 0);
            ArgbColor gray = new ArgbColor(255, 128, 128, 128);

            Console.WriteLine("Красный: " + red + " | uint: 0x" + red.ToRgb32().ToString("X8"));
            Console.WriteLine("Серый: " + gray + " | Является серым? " + gray.IsGray);
            Console.WriteLine("Индекс 1 (R) у красного: " + red[1]);
            
            uint packed = red;
            ArgbColor unpacked = 0xFF00FF00;
            Console.WriteLine("Преобразование uint: " + packed + " | Обратно: " + unpacked);
        }

        private static void AddColor(Palette palette)
        {
            Console.WriteLine("--- ДОБАВЛЕНИЕ ЦВЕТА ---");
            Console.WriteLine("Формат ввода: HEX-код (6 или 8 символов).");
            Console.WriteLine("Примеры: 'FF0000' (красный), '00FF00' (зеленый), '80FFFFFF' (полупрозрачный белый).");
            Console.WriteLine("Если ввести 6 символов, прозрачность (A) станет максимальной (FF).");
            Console.WriteLine();
            
            Console.Write("Введите имя цвета: ");
            string name = Console.ReadLine();

            if (name == null || name.Trim() == "")
            {
                Console.WriteLine("ОШИБКА: Имя не может быть пустым!");
                return;
            }

            Console.Write("Введите HEX-код: ");
            string hex = Console.ReadLine().Trim();

            if (hex.Length != 6 && hex.Length != 8)
            {
                Console.WriteLine("ОШИБКА: HEX-код должен быть длиной 6 или 8 символов.");
                return;
            }

            bool isHex = true;
            foreach (char c in hex)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                {
                    isHex = false;
                    break;
                }
            }

            if (!isHex)
            {
                Console.WriteLine("ОШИБКА: HEX-код может содержать только цифры и буквы A-F.");
                return;
            }

            try
            {
                if (hex.Length == 6) hex = "FF" + hex;
                uint val = Convert.ToUInt32(hex, 16);
                ArgbColor c = val;
                
                palette.Add(name, c);
                Console.WriteLine("УСПЕХ: Цвет '" + name + "' добавлен (" + c + ")");
            }
            catch
            {
                Console.WriteLine("ОШИБКА: Не удалось добавить цвет.");
            }
        }

        private static void FindColor(Palette palette)
        {
            Console.Write("Введите имя для поиска: ");
            string name = Console.ReadLine();

            ArgbColor found = palette[name];
            if (found != null) Console.WriteLine("НАЙДЕНО: " + found);
            else Console.WriteLine("НЕ НАЙДЕНО: Цвета с таким именем нет.");
        }

        private static void FindPrefix(Palette palette)
        {
            Console.Write("Введите начало имени (префикс): ");
            string prefix = Console.ReadLine();

            var list = palette.GetByPrefix(prefix);
            
            if (list.Count == 0)
            {
                Console.WriteLine("НЕ НАЙДЕНО: Нет цветов, начинающихся с '" + prefix + "'.");
            }
            else
            {
                Console.WriteLine("НАЙДЕНО (" + list.Count + "):");
                foreach (var item in list)
                {
                    Console.WriteLine(" - " + item.name + " : " + item.color);
                }
            }
        }

        private static void DeleteColor(Palette palette)
        {
            Console.WriteLine("--- УДАЛЕНИЕ ЦВЕТА ---");
            var all = palette.GetAll();
            
            if (all.Count == 0)
            {
                Console.WriteLine("Палитра пуста, удалять нечего.");
                return;
            }

            Console.WriteLine("Доступные цвета для удаления:");
            int i = 1;
            foreach (var item in all)
            {
                Console.WriteLine(" " + i + ". " + item.name + " (" + item.color + ")");
                i++;
            }
            Console.WriteLine();

            Console.Write("Введите имя цвета для удаления: ");
            string name = Console.ReadLine();

            if (palette.Remove(name))
                Console.WriteLine("УСПЕХ: Цвет '" + name + "' удален.");
            else
                Console.WriteLine("НЕ НАЙДЕНО: Цвета с таким именем нет.");
        }

        private static void TestErrors()
        {
            Console.WriteLine("Тест исключений:");
            
            try
            {
                ArgbColor c = new ArgbColor(255, 0, 0, 0);
                Console.Write("Попытка взять канал 10... ");
                byte bad = c[10]; 
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine("ПОЙМАНО: " + e.Message);
            }

            try
            {
                Palette p = new Palette();
                Console.Write("Попытка добавить пустое имя... ");
                p.Add("", new ArgbColor(255, 1, 2, 3));
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("ПОЙМАНО: " + e.Message);
            }
        }
    }
}