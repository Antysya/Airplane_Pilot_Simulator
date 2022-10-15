using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static System.Console;


namespace Airplane_Pilot_Simulator
{
    #region Описание движения самолета
    /*класс «Самолет» (в процессе тренировки пилотов самолета используется только один объект самолета). Самолет может изменять скорость и высоту.
            • Скорость — изменяется клавишами-стрелками Left и Right:
    (Right: +50 км/ч, Left: –50 км/ч, Shift-Right: +150 км/ч, Shift-Left: –150 км/ч).
            • Высота — изменяется клавишами-стрелками Up и Down: (Up: +250 м, Down: –250 м, Shift-Up: +500 м, Shift-Down: –500 м).*/
    #endregion
    [Serializable]
    public class Plane
    {
        private int Speed; //скорость
        private int Height; //высота
        [NonSerialized]
        private List<Dispatcher> dispatchers; //лист диспетчеров
        [NonSerialized]
        private bool IsSpeedGained; //проверка набора необходимой скорости
        [NonSerialized]
        private bool IsFlyBegin; //начало полета
        private int Penalties; //штрафы

        //конструктор начального состояния самолета
        public Plane()
        {
            Speed = 0;
            Height = 0;
            dispatchers = new List<Dispatcher>();
            IsSpeedGained = false;
            IsFlyBegin = false;
            Penalties = 0;
        }
        public override string ToString()
        {
            return $"\tСкорость: {Speed} км/ч\n\tВысота: {Height} м\n\tШтрафы: {Penalties}";
        }

        private event Action<int, int, int> Change;

        //добавить 2х диспетчеров и присвоить имена
        public void AddDispatchers()
        {
            WriteLine("Введите имя первого диспетчера:");
            string name = ReadLine();
            Dispatcher d1 = new Dispatcher(name);
            dispatchers.Add(d1);
            WriteLine($"Диспетчер {name} добавлен!");

            WriteLine("Введите имя второго диспетчера:");
            string name2 = ReadLine();
            Dispatcher d2 = new Dispatcher(name2);
            dispatchers.Add(d2);
            WriteLine($"Диспетчер {name2} добавлен!");

        }

        #region Управление
        //полет
        /*   • Скорость — изменяется клавишами-стрелками Left и Right: (Right: +50 км/ч, Left: –50 км/ч, Shift-Right: +150 км/ч, Shift-Left: –150 км/ч).
            • Высота — изменяется клавишами-стрелками Up и Down: (Up: +250 м, Down: –250 м, Shift-Up: +500 м, Shift-Down: –500 м).*/
        #endregion

        public void Fly(Plane plane)
        {
            int k = 0;
            while (true)
            {
                ConsoleKeyInfo key = ReadKey();
                Clear();

                if ((key.Modifiers & ConsoleModifiers.Shift) != 0)
                {
                    if (key.Key == ConsoleKey.RightArrow) Speed += 150;
                    else if (key.Key == ConsoleKey.LeftArrow) Speed -= 150;
                    else if (key.Key == ConsoleKey.UpArrow) Height += 500;
                    else if (key.Key == ConsoleKey.DownArrow) Height -= 500;
                }
                else
                {
                    if (key.Key == ConsoleKey.RightArrow) Speed += 50;
                    else if (key.Key == ConsoleKey.LeftArrow) Speed -= 50;
                    else if (key.Key == ConsoleKey.UpArrow) Height += 250;
                    else if (key.Key == ConsoleKey.DownArrow) Height -= 250;
                }

                if (Speed > 50)// Управление самолетом диспетчерами начинается
                {
                    WriteLine();
                    if (!IsFlyBegin) // Оповещение о начале полета
                    {
                        SetCursorPosition(40, 2);
                        WriteLine("Полет начался!\a");
                        SetCursorPosition(25, 3);
                        WriteLine("Следите за рекомендациями диспетчеров!");
                    }
                    IsFlyBegin = true;

                    Change += dispatchers[k].RecommendedFlightAltitude;
                    // В процессе полета самолет автоматически сообщает всем диспетчерам все изменения в скорости и высоте полета с помощью делегатов
                    Change(Speed, Height, Penalties);


                    if (Speed == 1000 && k == 0)
                    {
                        Change -= dispatchers[k].RecommendedFlightAltitude;
                        k = 1;
                        Change += dispatchers[k].RecommendedFlightAltitude;
                        Change(Speed, Height, Penalties);
                        IsSpeedGained = true;
                        SetCursorPosition(20, 4);
                        WriteLine("Вы набрали максимальную скорость. Ваша задача - посадить самолет!");
                    }
                }
                else if (IsSpeedGained && Speed <= 50)// Завершение полета
                {
                    Clear();
                    WriteLine("\n\tПолет закончился!\a");
                    WriteLine("Штрафные баллы за игру:");
                    Penalties = 0;
                    foreach (Dispatcher i in dispatchers)
                    {
                        Penalties += i.ChargeFine;
                        WriteLine($"Диспетчер {i.Name}: {i.ChargeFine}");
                    }
                    WriteLine($"Сумарное число штрафных очков: {Penalties}\a");
                    break;// Выход из цикла
                }
 
                Display.PlanP();
                SetCursorPosition(10, 18);
                WriteLine($"Скорость: {Speed} км/ч Высота: {Height} м");
                BinaryFormatter pf = new BinaryFormatter();
                BinaryFormatter df = new BinaryFormatter();
                using (Stream fstream = File.OpenWrite("Черный ящик.bin"))
                {
                    pf.Serialize(fstream, plane);
                }
                using (Stream fstream = File.OpenWrite("Рекомендации диспетчера.bin"))
                {
                    df.Serialize(fstream, dispatchers[k]);
                }
                Change -= dispatchers[k].RecommendedFlightAltitude;
                Penalties = dispatchers[0].ChargeFine + dispatchers[1].ChargeFine;
            }
        }

        public static void BlackBox()
        {
            BinaryFormatter pf = new BinaryFormatter();
            WriteLine("\tЧЕРНЫЙ ЯЩИК");
            using (Stream fstream = File.OpenRead("Черный ящик.bin"))
            {
                WriteLine("\nПоследние данные борта:");
                Plane p = (Plane)pf.Deserialize(fstream);
                WriteLine(p);
            }
            BinaryFormatter df = new BinaryFormatter();
            using (Stream fstream = File.OpenRead("Рекомендации диспетчера.bin"))
            {
                WriteLine("\nПоследние Рекомендации диспетчера:");
                Dispatcher d = (Dispatcher)df.Deserialize(fstream);
                WriteLine(d);
            }
            ReadLine();
        }
    }
}

