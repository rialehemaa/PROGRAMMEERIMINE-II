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

        // числа для игри с лимитом по сложности(1-4/5-9/10-12 класс)
        public void GeneraeriUus()
        {
            if (Tehe == '+')
            {
                // tuutoriali muster: mõlemad liidetavad ühe Next() kutsega
                int maxLiidetav = 20;
                if (tase == 1) maxLiidetav = 100;
                if (tase == 2) maxLiidetav = 999;

                A = rnd.Next(maxLiidetav + 1);
                B = rnd.Next(maxLiidetav + 1);
                Vastus = A + B;
            }
            else if (Tehe == '-')
            {
                // tuutoriali muster: subtrahend = Next(1, minuend), seega alati 1 kuni minuend-1
                int maxVahendatav = 20;
                if (tase == 1) maxVahendatav = 100;
                if (tase == 2) maxVahendatav = 999;

                A = rnd.Next(1, maxVahendatav + 1);
                B = rnd.Next(1, A);
                Vastus = A - B;
            }
            else if (Tehe == '*')
            {
                // tuutoriali muster: Next(min, max)
                int minTegur = 1;
                int maxTegur = 10;
                if (tase == 1) { minTegur = 2; maxTegur = 20; }
                if (tase == 2) { minTegur = 10; maxTegur = 99; }

                A = rnd.Next(minTegur, maxTegur + 1);
                B = rnd.Next(minTegur, maxTegur + 1);
                Vastus = A * B;
            }
            else if (Tehe == '/')
            {
                // tuutoriali muster: divisor ja jagatis Next(min, max), dividend = divisor * jagatis
                int minJag = 1;
                int maxJag = 10;
                if (tase == 1) { minJag = 2; maxJag = 20; }
                if (tase == 2) { minJag = 10; maxJag = 50; }

                B = rnd.Next(minJag, maxJag + 1);
                Vastus = rnd.Next(minJag, maxJag + 1);
                A = B * Vastus;
            }
        }

        public bool Kontrolli(int vastus)
        {
            return vastus == Vastus;
        }
    }
}