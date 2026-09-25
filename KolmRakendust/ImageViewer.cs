using System.Collections.Generic;
using System.Drawing;

namespace KolmRakendust
{
    public class PictureManager
    {
        //Пример инкапсуляции: приватный список путей и индекс,
        private List<string> pildid = new List<string>();
        private int indeks = -1;

        public int Count
        {
            get { return pildid.Count; }
        }

        public string PraeguneTee
        {
            get
            {
                if (indeks < 0 || indeks >= pildid.Count) return null;
                return pildid[indeks];
            }
        }

        public void Lisa(string tee)
        {
            pildid.Add(tee);
            indeks = pildid.Count - 1;
        }

        public void Tyhjenda()
        {
            pildid.Clear();
            indeks = -1;
        }

        public string Jargmine()
        {
            if (pildid.Count == 0) return null;
            indeks++;
            if (indeks >= pildid.Count) indeks = 0;
            return PraeguneTee;
        }
    }
}