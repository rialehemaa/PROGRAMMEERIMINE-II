using System.Drawing;
using System.Windows.Forms;

namespace KolmRakendust
{
    // Ühine kujundus kõigile vormidele - beeþ värvipalett ja lamedad nupud.
    // Iga vorm kutsub siit meetodeid, et kogu rakendus näeks ühtne välja.
    public static class Teema
    {
        public static Color Taust = Color.FromArgb(245, 238, 224);
        public static Color Paneel = Color.FromArgb(235, 224, 202);
        public static Color NupuVarv = Color.FromArgb(216, 199, 163);
        public static Color NupuAareVarv = Color.FromArgb(150, 121, 82);
        public static Color TekstiVarv = Color.FromArgb(74, 58, 42);
        public static Color KaardiVarv = Color.FromArgb(223, 205, 169);
        public static Color RohelineVarv = Color.FromArgb(107, 142, 94);
        public static Color PunaneVarv = Color.FromArgb(176, 92, 68);

        public static void StiliseeriVorm(Form vorm)
        {
            vorm.BackColor = Taust;
        }

        public static void StiliseeriNupp(Button nupp)
        {
            nupp.FlatStyle = FlatStyle.Flat;
            nupp.FlatAppearance.BorderColor = NupuAareVarv;
            nupp.FlatAppearance.BorderSize = 1;
            nupp.BackColor = NupuVarv;
            nupp.ForeColor = TekstiVarv;
            nupp.Cursor = Cursors.Hand;
        }

        public static void StiliseeriGrupp(GroupBox grupp)
        {
            grupp.ForeColor = TekstiVarv;
        }
    }
}