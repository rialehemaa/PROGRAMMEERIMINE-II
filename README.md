Kolm rakendust (Windows Forms)
Projekti eesmärk ja kirjeldus

Pildivaataja — kuvab pilti, võimaldab muuta tausta värvi, venitust ning esitada pilte slaidiesitusena (kui lisatud on 2 või rohkem fotot).
Matemaatiline mäng — genereerib ülesandeid valitud klassi ja aja järgi ning kontrollib kasutaja vastuseid.
Sarnaste piltide leidmise mäng — mälumäng, kus tuleb leida sümbolite paarid valitud suurusega mängulaual.
Kasutatud tehnoloogiad:
C#
Windows Forms (.NET Framework 4.7.2)
OOP (klassid, kapseldamine — vormid ei sisalda äriloogikat otse, vaid kasutavad selleks eraldi klasse)
1. Pildivaataja

Arendusideed:
Lisa nupu abil pildi salvestamine teise formaati (Image.Save). — ei ole veel tehtud
Lisa pildigalerii pisipiltidega, et valitud pilte kiiremini sirvida. — ei ole veel tehtud
Lisa slaidiesituse kiiruse valik (nt 1 / 2 / 5 sekundit). — ei ole veel tehtud

Tehtud algülesandest:
Taustavärvi muutmine (ColorDialog) — olemas, BtnTaust_Click
Slaidiesitus automaatse vahetusega — olemas, märkeruut chkSlaid + slaidTimer (2 sek)

Lisatud omalt poolt:
Mitme pildi korraga laadimine (Multiselect), pildi pööramine 90° (BtnPoorra_Click), pildi peegeldamine (BtnPeegelda_Click).

2. Matemaatiline mäng

Arendusideed:
Lisa punktiarvestus, mis arvestab ka vastamise kiirust. — ei ole veel tehtud
Lisa tulemuste salvestamine faili, et näha oma parimaid katseid. — ei ole veel tehtud
Lisa rohkem ülesandeid korraga (nt 8 tehet 4 asemel) raskema taseme puhul. — ei ole veel tehtud

Tehtud algülesandest:
Erinevad tehted (−, ×, ÷) — olemas, MathProblem töötleb +, -, *, /
Ajapiirang — olemas (timer, sekundeid); punktisüsteem — ei ole tehtud, on ainult loendur "mitu õiget 4-st" mängu lõpus, mitte kogunevad punktid
Raskusastme valik — olemas (1.-4. / 5.-9. / 10.-12. klass)

Lisatud omalt poolt:
Aja kestuse valik (30/45/60 sekundit), nupp "Kontrolli vastuseid" mängu enneaegseks lõpetamiseks, õige/vale märgistus iga vastuse juures.

3. Sarnaste piltide leidmise mäng

Arendusideed:
Lisa tegelikud pildid sümbolite asemel (nt PictureBox koos peidetud pildiga). — ei ole veel tehtud
Lisa punktisüsteem (vähem käike/aega = rohkem punkte). — ei ole veel tehtud
Lisa helid paari sobimisel/mittesobimisel (SoundPlayer). — ei ole veel tehtud

Tehtud algülesandest:
Taimer — olemas (varjaTimer kaartide peitmiseks + aegTimer sekundikellake); punktisüsteem — ei ole tehtud, ainult aeg sekundites
Tasemed (4x4, 6x6) — olemas, grpSuurus koos rb4/rb6

Lisatud omalt poolt:
Nähtav sekundikellake kogu mängu kestuse mõõtmiseks, tagasipöördumine suuruse valiku ekraanile pärast võitu.

Eeldav edasiareng:
Kõik kolm rakendust saab siduda ühise punktiarvestuse/kasutajaprofiiliga, lisada helid (SoundPlayer) sündmustele (õige vastus, paari leidmine) ning salvestada tulemused failisse, et näha ajaloolist statistikat.
