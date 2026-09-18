using System;

namespace KolmRakendust
{
    public class MathProblem
    {
        public int A { get; private set; }
        public int B { get; private set; }
        public char Tehe { get; private set; }
        public int Vastus { get; private set; }

        private static Random rnd = new Random();
        private int tase;

        // tase: 0 = 1.-4. klass, 1 = 5.-9. klass, 2 = 10.-12. klass
        public MathProblem(char tehe, int tase)
        {
            Tehe = tehe;
            this.tase = tase;
            GeneraeriUus();
        }

        public void GeneraeriUus()
        {
            switch (Tehe)
            {
                case '+':
                    {
                        // tuutoriali muster: mõlemad liidetavad ühe Next() kutsega
                        int maxLiidetav = tase == 0 ? 20 : (tase == 1 ? 100 : 999);
                        A = rnd.Next(maxLiidetav + 1);
                        B = rnd.Next(maxLiidetav + 1);
                        Vastus = A + B;
                        break;
                    }

                case '-':
                    {
                        // tuutoriali muster: subtrahend = Next(1, minuend), seega alati 1 kuni minuend-1
                        int maxVahendatav = tase == 0 ? 20 : (tase == 1 ? 100 : 999);
                        A = rnd.Next(1, maxVahendatav + 1);
                        B = rnd.Next(1, A);
                        Vastus = A - B;
                        break;
                    }

                case '*':
                    {
                        // tuutoriali muster: Next(min, max)
                        int minTegur = tase == 0 ? 1 : (tase == 1 ? 2 : 10);
                        int maxTegur = tase == 0 ? 10 : (tase == 1 ? 20 : 99);
                        A = rnd.Next(minTegur, maxTegur + 1);
                        B = rnd.Next(minTegur, maxTegur + 1);
                        Vastus = A * B;
                        break;
                    }

                case '/':
                    {
                        // tuutoriali muster: divisor ja jagatis Next(min, max), dividend = divisor * jagatis
                        int minJag = tase == 0 ? 1 : (tase == 1 ? 2 : 10);
                        int maxJag = tase == 0 ? 10 : (tase == 1 ? 20 : 50);
                        B = rnd.Next(minJag, maxJag + 1);
                        Vastus = rnd.Next(minJag, maxJag + 1);
                        A = B * Vastus;
                        break;
                    }
            }
        }

        public bool Kontrolli(int vastus)
        {
            return vastus == Vastus;
        }
    }
}