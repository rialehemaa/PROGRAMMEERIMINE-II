using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tooded_DB
{
    // Kolm keelt: eesti (algtekst), vene, inglise. Valik salvestatakse faili keel.txt.
    public static class Keel
    {
        public static string Praegune = "et";   // et / ru / en

        private static readonly string Fail = Path.Combine(Kaustad.Juur, "keel.txt");

        private static readonly Dictionary<string, (string ru, string en)> S = new Dictionary<string, (string ru, string en)>
        {
            ["Teade"] = ("Сообщение", "Message"),
            ["OTSAS"] = ("НЕТ В НАЛИЧИИ", "SOLD OUT"),
            ["Kinnitus"] = ("Подтверждение", "Confirmation"),
            ["Peamenüü"] = ("Главное меню", "Main menu"),
            ["Kassa"] = ("Касса", "Cash desk"),
            ["Kõik"] = ("Все", "All"),
            ["Pood"] = ("Магазин", "Shop"),
            ["Ladu (haldus)"] = ("Склад (управление)", "Warehouse (manage)"),
            ["Kliendid (admin)"] = ("Клиенты (админ)", "Customers (admin)"),
            ["Müügiajalugu"] = ("История продаж", "Sales history"),
            ["Veebipood"] = ("Интернет-магазин", "Online shop"),
            ["Pakkumised (haldus)"] = ("Акции (управление)", "Offers (manage)"),
            ["Nädala pakkumised"] = ("Предложения недели", "Weekly offers"),
            ["Ostunimekiri"] = ("Список покупок", "Shopping list"),
            ["Minu ostunimekiri"] = ("Мой список покупок", "My shopping list"),
            ["Ostuajalugu"] = ("История покупок", "Purchase history"),
            ["Logi välja"] = ("Выйти", "Log out"),
            ["Logi sisse"] = ("Войти", "Log in"),
            ["Registreeri"] = ("Регистрация", "Register"),
            ["Registreerimine"] = ("Регистрация", "Registration"),
            ["Sisselogimine"] = ("Вход", "Login"),
            ["Pood – sisselogimine"] = ("Магазин – вход", "Shop – login"),
            ["Uus kasutaja"] = ("Новый пользователь", "New user"),
            ["Kasutajanimi"] = ("Имя пользователя", "Username"),
            ["Parool"] = ("Пароль", "Password"),
            ["Parool (vähemalt 4 märki)"] = ("Пароль (минимум 4 символа)", "Password (at least 4 characters)"),
            ["Korda parooli"] = ("Повторите пароль", "Repeat password"),
            ["Roll"] = ("Роль", "Role"),
            ["Toodete haldus"] = ("Управление товарами", "Product management"),
            ["Kliendid – admin paneel"] = ("Клиенты – панель администратора", "Customers – admin panel"),
            ["Kliendid – uue kliendi lisamine"] = ("Клиенты – добавление нового клиента", "Customers – add a new customer"),
            ["Kategooria:"] = ("Категория:", "Category:"),
            ["Ostukorv"] = ("Корзина", "Cart"),
            ["Eemalda"] = ("Убрать", "Remove"),
            ["Tühjenda"] = ("Очистить", "Clear"),
            ["Tühista"] = ("Отмена", "Cancel"),
            ["Otsi"] = ("Найти", "Search"),
            ["Kaart / nimi:"] = ("Карта / имя:", "Card / name:"),
            ["Kliendikaart / nimi:"] = ("Карта клиента / имя:", "Customer card / name:"),
            ["Klienti pole valitud"] = ("Клиент не выбран", "No customer selected"),
            ["Kasuta boonust"] = ("Использовать бонус", "Use bonus"),
            ["Makseviis:"] = ("Оплата:", "Payment:"),
            ["Maksa ja loo tšekk (PDF)"] = ("Оплатить и создать чек (PDF)", "Pay and create receipt (PDF)"),
            ["Telli ja maksa"] = ("Заказать и оплатить", "Order and pay"),
            ["Saada viimane tšekk e-mailile"] = ("Отправить последний чек на e-mail", "E-mail the last receipt"),
            ["Müüja: "] = ("Продавец: ", "Seller: "),
            ["Klient: "] = ("Клиент: ", "Customer: "),
            ["Kokku: "] = ("Итого: ", "Total: "),
            ["Soodus kokku: "] = ("Скидка всего: ", "Total discount: "),
            ["Boonus: "] = ("Бонус: ", "Bonus: "),
            ["Maksta: "] = ("К оплате: ", "To pay: "),
            ["Tagasi anda: "] = ("Сдача: ", "Change: "),
            ["Sularahas maksmine"] = ("Оплата наличными", "Cash payment"),
            ["Klient andis (€):"] = ("Клиент дал (€):", "Customer gave (€):"),
            ["Antud summa on liiga väike"] = ("Дана слишком маленькая сумма", "The amount given is too small"),
            ["Antud summa on väiksem kui maksta."] = ("Дана сумма меньше, чем к оплате.", "The amount given is less than the total."),
            ["Kaardimakse"] = ("Оплата картой", "Card payment"),
            ["Kaardi number:"] = ("Номер карты:", "Card number:"),
            ["Kehtib (KK/AA):"] = ("Срок действия (ММ/ГГ):", "Valid thru (MM/YY):"),
            ["CVC:"] = ("CVC:", "CVC:"),
            ["Maksa"] = ("Оплатить", "Pay"),
            ["Vali klient"] = ("Выберите клиента", "Select customer"),
            ["Vali"] = ("Выбрать", "Select"),
            ["Lisa"] = ("Добавить", "Add"),
            ["Kogus: "] = ("Количество: ", "Quantity: "),
            ["Hind: "] = ("Цена: ", "Price: "),
            ["Kliendikaart"] = ("Карта клиента", "Customer card"),
            ["Sisesta kliendikaardi number või nimi:"] = ("Введите номер карты или имя клиента:", "Enter the customer card number or name:"),
            ["Sisesta kliendikaardi number või nimi."] = ("Введите номер карты или имя клиента.", "Enter the customer card number or name."),
            ["Klienti ei leitud."] = ("Клиент не найден.", "Customer not found."),
            ["Klienti ei leitud enam andmebaasist."] = ("Клиента больше нет в базе данных.", "The customer is no longer in the database."),
            ["Ostukorv on tühi."] = ("Корзина пуста.", "The cart is empty."),
            ["Esmalt tee ost, et tekiks tšekk."] = ("Сначала сделайте покупку, чтобы появился чек.", "Make a purchase first to get a receipt."),
            ["Kontrolli kaardi andmeid (number, kehtivus, CVC)."] = ("Проверьте данные карты (номер, срок, CVC).", "Check the card details (number, expiry, CVC)."),
            ["Laost ei piisa"] = ("Недостаточно на складе", "Not enough in stock"),
            ["Valmis"] = ("Готово", "Done"),
            ["Tšekk saadetud: "] = ("Чек отправлен: ", "Receipt sent: "),
            ["Andmebaasi viga: "] = ("Ошибка базы данных: ", "Database error: "),
            ["E-maili saatmine ebaõnnestus:\n"] = ("Не удалось отправить e-mail:\n", "Sending the e-mail failed:\n"),
            ["Ostu ei õnnestunud sooritada (laoseisu ei muudetud):\n"] = ("Покупку выполнить не удалось (склад не изменён):\n", "The purchase failed (stock was not changed):\n"),
            ["Saada tšekk"] = ("Отправить чек", "Send receipt"),
            ["Sisesta saaja e-mail:"] = ("Введите e-mail получателя:", "Enter the recipient's e-mail:"),
            ["OK"] = ("OK", "OK"),
            ["Soodused"] = ("Скидки", "Discounts"),
            ["Komplektid"] = ("Комплекты", "Bundles"),
            ["Soodus (%)"] = ("Скидка (%)", "Discount (%)"),
            ["Alates"] = ("С", "From"),
            ["Kuni"] = ("По", "Until"),
            ["Staatus"] = ("Статус", "Status"),
            ["Sea soodus"] = ("Установить скидку", "Set discount"),
            ["Kustuta soodus"] = ("Удалить скидку", "Delete discount"),
            ["Lõpeta kohe"] = ("Завершить сейчас", "End now"),
            ["Vali tabelist toode"] = ("Выберите товар в таблице", "Select a product in the table"),
            ["Valitud: "] = ("Выбрано: ", "Selected: "),
            ["Tüüp"] = ("Тип", "Type"),
            ["Toode"] = ("Товар", "Product"),
            ["Teine toode"] = ("Второй товар", "Second product"),
            ["Kogus (N)"] = ("Количество (N)", "Quantity (N)"),
            ["Tasuta (M)"] = ("Бесплатно (M)", "Free (M)"),
            ["Komplekti hind (€)"] = ("Цена комплекта (€)", "Bundle price (€)"),
            ["Teise toote soodus (%)"] = ("Скидка на второй товар (%)", "Second product discount (%)"),
            ["Lisa uus"] = ("Добавить новую", "Add new"),
            ["Salvesta muudatused"] = ("Сохранить изменения", "Save changes"),
            ["Kustuta valitud"] = ("Удалить выбранное", "Delete selected"),
            ["Pakkumine"] = ("Предложение", "Offer"),
            ["Kategooria"] = ("Категория", "Category"),
            ["Tavahind"] = ("Обычная цена", "Regular price"),
            ["Uus hind"] = ("Новая цена", "New price"),
            ["Soodus"] = ("Скидка", "Discount"),
            ["Aktiivse pakkumise „Kuni” ei tohi olla minevikus."] = ("У активного предложения дата «Kuni» не может быть в прошлом.", "An active offer's “Until” date cannot be in the past."),
            ["„Kuni” ei tohi olla enne „Alates” kuupäeva."] = ("Дата «Kuni» не может быть раньше «Alates».", "“Until” cannot be before “From”."),
            ["Kuupäev ei tohi olla minevikus."] = ("Дата не может быть в прошлом.", "The date cannot be in the past."),
            ["Komplektis peab olema vähemalt 2 toodet."] = ("В комплекте должно быть минимум 2 товара.", "A bundle needs at least 2 items."),
            ["Vali teine toode (see peab olema erinev)."] = ("Выберите второй товар (он должен отличаться).", "Select a second product (it must be different)."),
            ["Vali toode."] = ("Выберите товар.", "Select a product."),
            ["Vali tabelist toode."] = ("Выберите товар в таблице.", "Select a product in the table."),
            ["Vali tabelist pakkumine."] = ("Выберите предложение в таблице.", "Select an offer in the table."),
            ["Pakkumist ei leitud."] = ("Предложение не найдено.", "Offer not found."),
            ["Sellel tootel pole soodust."] = ("У этого товара нет скидки.", "This product has no discount."),
            ["Toodet ei leitud."] = ("Товар не найден.", "Product not found."),
            ["Pakkumisi saab seada ainult müüja või omanik."] = ("Предложения могут задавать только продавец или владелец.", "Only the seller or owner can set offers."),
            ["Sellel vormil pole sul juurdepääsu."] = ("У вас нет доступа к этому окну.", "You do not have access to this window."),
            ["Toote nimetus"] = ("Название товара", "Product name"),
            ["Hind €"] = ("Цена €", "Price €"),
            ["Hind € (tk või kg kohta)"] = ("Цена € (за шт или кг)", "Price € (per pc or kg)"),
            ["Kogus (kg-toote puhul kilogrammides)"] = ("Количество (для кг-товара в кг)", "Quantity (in kg for kg items)"),
            ["Lisa kategooria"] = ("Добавить категорию", "Add category"),
            ["Kustuta kategooria"] = ("Удалить категорию", "Delete category"),
            ["Otsi fail"] = ("Выбрать файл", "Browse file"),
            ["Uuenda"] = ("Обновить", "Update"),
            ["Kustuta"] = ("Удалить", "Delete"),
            ["Toodenimetus"] = ("Название", "Product name"),
            ["Kogus"] = ("Кол-во", "Qty"),
            ["Hind"] = ("Цена", "Price"),
            ["Uhik"] = ("Единица", "Unit"),
            ["Pilt"] = ("Фото", "Photo"),
            ["Sisesta toote nimetus!"] = ("Введите название товара!", "Enter the product name!"),
            ["Vali kategooria!"] = ("Выберите категорию!", "Select a category!"),
            ["Kas kustutada valitud kategooria?"] = ("Удалить выбранную категорию?", "Delete the selected category?"),
            ["Selles kategoorias on tooteid. Kustuta või liiguta tooted enne ära."] = ("В этой категории есть товары. Сначала удалите или переместите их.", "This category has products. Delete or move them first."),
            ["Selline kategooria on juba olemas!"] = ("Такая категория уже существует!", "That category already exists!"),
            ["Otsi (nimi / kaart):"] = ("Поиск (имя / карта):", "Search (name / card):"),
            ["Nimi"] = ("Имя", "Name"),
            ["E-post"] = ("E-mail", "E-mail"),
            ["Telefon"] = ("Телефон", "Phone"),
            ["Kliendikaart (tühi = automaatne)"] = ("Карта (пусто = авто)", "Card (empty = auto)"),
            ["Sünnipäev (tühi = teadmata)"] = ("День рождения (необяз.)", "Birthday (optional)"),
            ["Boonus (€)"] = ("Бонус (€)", "Bonus (€)"),
            ["Boonus"] = ("Бонус", "Bonus"),
            ["Synnipaev"] = ("День рождения", "Birthday"),
            ["Tühjenda väljad"] = ("Очистить поля", "Clear fields"),
            ["Sisesta kliendi nimi!"] = ("Введите имя клиента!", "Enter the customer name!"),
            ["Boonus peab olema arv!"] = ("Бонус должен быть числом!", "Bonus must be a number!"),
            ["Boonus ei tohi olla negatiivne!"] = ("Бонус не может быть отрицательным!", "Bonus cannot be negative!"),
            ["Selline kliendikaart on juba olemas!"] = ("Такая карта клиента уже существует!", "That customer card already exists!"),
            ["Vali tabelist klient."] = ("Выберите клиента в таблице.", "Select a customer in the table."),
            ["Kas kustutada valitud klient?"] = ("Удалить выбранного клиента?", "Delete the selected customer?"),
            ["Tagasta valitud ost"] = ("Вернуть выбранную покупку", "Return selected purchase"),
            ["Nr"] = ("№", "No."),
            ["Kuupäev"] = ("Дата", "Date"),
            ["Müüja"] = ("Продавец", "Seller"),
            ["Klient"] = ("Клиент", "Customer"),
            ["Kokku"] = ("Итого", "Total"),
            ["Sünnipäeva soodus"] = ("Скидка на день рождения", "Birthday discount"),
            ["Makseviis"] = ("Способ оплаты", "Payment method"),
            ["Makstud"] = ("Оплачено", "Paid"),
            ["Arve"] = ("Чек", "Receipt"),
            ["Summa"] = ("Сумма", "Amount"),
            ["Ostu kaubad (topeltklõps ostule avab tšeki PDF-i)"] = ("Товары покупки (двойной щелчок по покупке открывает чек PDF)", "Items of the purchase (double-click a purchase to open its PDF receipt)"),
            ["Kas tagastada valitud ost? Kaubad lähevad tagasi lattu."] = ("Вернуть выбранную покупку? Товары вернутся на склад.", "Return the selected purchase? The items go back to stock."),
            ["Tagastus"] = ("Возврат", "Return"),
            ["Ost on tagastatud."] = ("Покупка возвращена.", "The purchase has been returned."),
            ["See ost on juba tagastatud."] = ("Эта покупка уже возвращена.", "This purchase has already been returned."),
            ["Selle vana ostu ridu pole salvestatud, tagastada ei saa."] = ("Строки этой старой покупки не сохранены, вернуть нельзя.", "This old purchase has no saved lines and cannot be returned."),
            ["Vali tabelist ost."] = ("Выберите покупку в таблице.", "Select a purchase in the table."),
            ["Ostu ei leitud."] = ("Покупка не найдена.", "Purchase not found."),
            ["Tagastatud"] = ("Возвращено", "Returned"),
            ["PDF-faili ei leitud."] = ("PDF-файл не найден.", "PDF file not found."),
            ["PDF-faili ei leitud kaustast Arved."] = ("PDF-файл не найден в папке Arved.", "PDF file not found in the Arved folder."),
            ["Kogus:"] = ("Количество:", "Quantity:"),
            ["Lisa nimekirja →"] = ("Добавить в список →", "Add to list →"),
            ["Sisesta kasutajanimi ja parool!"] = ("Введите имя пользователя и пароль!", "Enter username and password!"),
            ["Sellist kasutajat pole!"] = ("Такого пользователя нет!", "No such user!"),
            ["Vale parool!"] = ("Неверный пароль!", "Wrong password!"),
            ["Kasutaja loodud. Logi sisse."] = ("Пользователь создан. Войдите.", "User created. Please log in."),
            ["Kasutajanimi peab olema vähemalt 3 märki!"] = ("Имя пользователя — минимум 3 символа!", "Username must be at least 3 characters!"),
            ["Parool peab olema vähemalt 4 märki!"] = ("Пароль — минимум 4 символа!", "Password must be at least 4 characters!"),
            ["Paroolid ei ole ühesugused!"] = ("Пароли не совпадают!", "Passwords do not match!"),
            ["Selline kasutajanimi on juba olemas!"] = ("Такое имя пользователя уже существует!", "That username already exists!"),
            ["Statistika"] = ("Статистика", "Statistics"),
            ["Periood:"] = ("Период:", "Period:"),
            ["Viimased 7 päeva"] = ("Последние 7 дней", "Last 7 days"),
            ["Viimased 30 päeva"] = ("Последние 30 дней", "Last 30 days"),
            ["Viimased 90 päeva"] = ("Последние 90 дней", "Last 90 days"),
            ["Kogu aeg"] = ("Всё время", "All time"),
            ["Müük päevade kaupa"] = ("Продажи по дням", "Sales by day"),
            ["Enimmüüdud tooted"] = ("Самые продаваемые товары", "Best-selling products"),
            ["Käive päevade kaupa (€)"] = ("Оборот по дням (€)", "Revenue by day (€)"),
            ["Enimmüüdud tooted (tulu €)"] = ("Самые продаваемые товары (выручка €)", "Best-selling products (revenue €)"),
            ["Andmeid pole."] = ("Данных нет.", "No data."),
            ["Müüke kokku:"] = ("Всего продаж:", "Total sales:"),
            ["Käive:"] = ("Оборот:", "Revenue:"),
            ["Keskmine ost:"] = ("Средний чек:", "Average sale:"),
            ["Tagastatud oste ei arvestata."] = ("Возвраты не учитываются.", "Returns are not counted."),
            ["Kuupäev"] = ("Дата", "Date"),
            ["Müüke"] = ("Продаж", "Sales"),
            ["Käive (€)"] = ("Оборот (€)", "Revenue (€)"),
            ["Tulu (€)"] = ("Выручка (€)", "Revenue (€)"),
            ["Tume teema"] = ("Тёмная тема", "Dark theme"),
            ["Hele teema"] = ("Светлая тема", "Light theme")
        };

        static Keel()
        {
            try
            {
                if (File.Exists(Fail))
                {
                    string k = File.ReadAllText(Fail).Trim();
                    if (k == "ru" || k == "en" || k == "et") Praegune = k;
                }
            }
            catch { }
        }

        public static void Sea(string keel)
        {
            Praegune = keel;
            try { Directory.CreateDirectory(Kaustad.Juur); File.WriteAllText(Fail, keel); } catch { }
        }

        // Toodete, kategooriate ja staatuste nimed (tõlgitakse ka tabelites, ripploendites ja kaartidel)
        private static readonly Dictionary<string, (string ru, string en)> P = new Dictionary<string, (string ru, string en)>
        {
            ["Coca-Cola Zero"] = ("Coca-Cola Zero", "Coca-Cola Zero"),
            ["Ice Tea"] = ("Холодный чай", "Iced tea"),
            ["Mahl"] = ("Сок", "Juice"),
            ["Mineraalvesi"] = ("Минеральная вода", "Mineral water"),
            ["Mineraalvesi gaseeritud"] = ("Минеральная вода газированная", "Sparkling mineral water"),
            ["Mineraalvesi gaseerimata"] = ("Минеральная вода негазированная", "Still mineral water"),
            ["Jogurt"] = ("Йогурт", "Yogurt"),
            ["Eesti juust"] = ("Эстонский сыр", "Estonian cheese"),
            ["Või"] = ("Сливочное масло", "Butter"),
            ["Piim"] = ("Молоко", "Milk"),
            ["Kohupiim"] = ("Творог", "Curd"),
            ["Kanafilee"] = ("Куриное филе", "Chicken fillet"),
            ["Hakkliha"] = ("Фарш", "Minced meat"),
            ["Vorstid"] = ("Колбаски", "Sausages"),
            ["Seakaelakarbonaad"] = ("Свиная шея (карбонад)", "Pork neck"),
            ["Sea liha"] = ("Свинина", "Pork"),
            ["Banaan"] = ("Бананы", "Bananas"),
            ["Apelsin"] = ("Апельсины", "Oranges"),
            ["Viinamarjad"] = ("Виноград", "Grapes"),
            ["Pirn"] = ("Груши", "Pears"),
            ["Õun"] = ("Яблоки", "Apples"),
            ["Maasikatort"] = ("Клубничный торт", "Strawberry cake"),
            ["Dubai tort"] = ("Дубайский торт", "Dubai cake"),
            ["Šokolaaditort"] = ("Шоколадный торт", "Chocolate cake"),
            ["Marjatort"] = ("Ягодный торт", "Berry cake"),
            ["Mango juustukook"] = ("Чизкейк с манго", "Mango cheesecake"),
            ["Champagne tort"] = ("Торт «Шампань»", "Champagne cake"),
            ["Juustukook"] = ("Чизкейк", "Cheesecake"),
            ["Sai"] = ("Белый хлеб", "White bread"),
            ["Rukkileib"] = ("Ржаной хлеб", "Rye bread"),
            ["Sepik"] = ("Хлеб «Сепик»", "Sepik bread"),
            ["Croissant"] = ("Круассан", "Croissant"),
            ["Kaerasai"] = ("Овсяный хлеб", "Oat bread"),
            ["Munad"] = ("Яйца", "Eggs"),
            ["Herned"] = ("Зелёный горошек", "Green peas"),
            ["Mais"] = ("Кукуруза", "Corn"),
            ["Tuunikala"] = ("Тунец", "Tuna"),
            ["Ketšup"] = ("Кетчуп", "Ketchup"),
            ["Makaronid"] = ("Макароны", "Pasta"),
            ["Riis"] = ("Рис", "Rice"),
            ["Tatar"] = ("Гречка", "Buckwheat"),
            ["Jahu"] = ("Мука", "Flour"),
            ["Suhkur"] = ("Сахар", "Sugar"),
            ["Šokolaad"] = ("Шоколад", "Chocolate"),
            ["Küpsised"] = ("Печенье", "Cookies"),
            ["Kommid"] = ("Конфеты", "Candies"),
            ["Kartulikrõpsud"] = ("Чипсы", "Chips"),
            ["Pähklid"] = ("Орехи", "Nuts"),
            // kategooriad
            ["Kondiitritooted"] = ("Кондитерские изделия", "Confectionery"),
            ["Liha"] = ("Мясо", "Meat"),
            ["Maiustused ja snäkid"] = ("Сладости и снеки", "Sweets and snacks"),
            ["Piimatooted"] = ("Молочные продукты", "Dairy"),
            ["Puuviljad"] = ("Фрукты", "Fruit"),
            ["Toidutooted"] = ("Продукты питания", "Groceries"),
            ["Joogid"] = ("Напитки", "Drinks"),
            ["Energiajoogid"] = ("Энергетики", "Energy drinks"),
            // staatused, makseviisid, tekstid
            ["Kehtib"] = ("Действует", "Active"),
            ["Tulemas"] = ("Скоро", "Upcoming"),
            ["Lõppenud"] = ("Закончилась", "Ended"),
            ["Aktiivne"] = ("Активна", "Active"),
            ["Lõpetatud"] = ("Завершена", "Finished"),
            ["Sularahas"] = ("Наличными", "Cash"),
            ["Kaardiga"] = ("Картой", "Card"),
            ["Arvega"] = ("По счёту", "Invoice"),
            ["Arvega (maksmata)"] = ("По счёту (не оплачено)", "Invoice (unpaid)"),
            ["Maksa kohe (siin ja praegu)"] = ("Оплата сразу (здесь и сейчас)", "Pay now (here and now)"),
            ["Tagastatud"] = ("Возвращено", "Refunded"),
            ["Sünnipäeva soodus"] = ("Скидка в день рождения", "Birthday discount"),
            ["Sünnipäevasoodus"] = ("Скидка в день рождения", "Birthday discount"),
            ["Osta N, saa M tasuta"] = ("Купи N, получи M бесплатно", "Buy N, get M free"),
            ["N tk hinnaga X €"] = ("N шт по цене X €", "N pcs for X €"),
            ["Osta toode, teine -% allahindlusega"] = ("Купи товар, второй со скидкой %", "Buy a product, get the second at a discount"),
            ["Tooted"] = ("Товары", "Products"),
            ["Müüke:"] = ("Продаж:", "Sales:"),
            ["Käive kokku:"] = ("Оборот:", "Turnover:"),
            ["(topeltklõps reale avab tšeki PDF-i)"] = ("(двойной щелчок открывает чек PDF)", "(double-click a row to open the receipt PDF)"),
            ["Kogus:"] = ("Количество:", "Quantity:"),
            ["Toode:"] = ("Товар:", "Product:"),
            ["Hind:"] = ("Цена:", "Price:"),
            ["Toote info"] = ("О товаре", "Product info"),
            ["Ost on sooritatud!"] = ("Покупка совершена!", "Purchase completed!"),
            ["Klient andis:"] = ("Клиент дал:", "Customer gave:"),
            ["TAGASI ANDA:"] = ("СДАЧА:", "CHANGE:"),
            ["Boonus:"] = ("Бонус:", "Bonus:"),
            ["kasutatud"] = ("использовано", "used"),
            ["saadud"] = ("получено", "earned"),
            ["kokku"] = ("всего", "total"),
            ["Tšekk on salvestatud:"] = ("Чек сохранён:", "Receipt saved:"),
            ["Kogus (kg-toote puhul kilogrammides)"] = ("Количество (для весового товара в кг)", "Quantity (in kg for weighed products)"),
            ["Hind € (tk või kg kohta)"] = ("Цена € (за шт или за кг)", "Price € (per pc or per kg)"),
            ["piiramatult"] = ("без ограничения", "unlimited"),
            ["Kassatšekk / Arve"] = ("Кассовый чек / Счёт", "Receipt / Invoice"),
            ["Kuupäev:"] = ("Дата:", "Date:"),
            ["Müüja:"] = ("Продавец:", "Seller:"),
            ["Klient:"] = ("Клиент:", "Customer:"),
            ["kaart"] = ("карта", "card"),
            ["Veebipood"] = ("Интернет-магазин", "Online shop"),
            ["tellija"] = ("заказчик", "customer"),
            ["Kogus"] = ("Кол-во", "Qty"),
            ["Summa"] = ("Сумма", "Amount"),
            ["Kokku:"] = ("Итого:", "Total:"),
            ["Pakkumised ja soodused:"] = ("Акции и скидки:", "Offers and discounts:"),
            ["(kogu ostukorv)"] = ("(вся корзина)", "(whole cart)"),
            ["Kasutatud boonus:"] = ("Использован бонус:", "Bonus used:"),
            ["Tasuda:"] = ("К оплате:", "To pay:"),
            ["Maksta:"] = ("К оплате:", "To pay:"),
            ["(7 päeva jooksul)"] = ("(в течение 7 дней)", "(within 7 days)"),
            ["Sääst kokku:"] = ("Экономия:", "You saved:"),
            ["Sularahas antud:"] = ("Дано наличными:", "Cash given:"),
            ["Tagasi:"] = ("Сдача:", "Change:"),
            ["Saadud boonus:"] = ("Получен бонус:", "Bonus earned:"),
            ["Boonus kokku:"] = ("Бонус всего:", "Total bonus:"),
            ["Arvega maksmine"] = ("Оплата по счёту", "Payment by invoice"),
            ["Saaja:"] = ("Получатель:", "Recipient:"),
            ["Viitenumber:"] = ("Номер ссылки:", "Reference number:"),
            ["Tähtaeg:"] = ("Срок:", "Due:"),
            ["Täname ostu eest!"] = ("Спасибо за покупку!", "Thank you for your purchase!"),
            ["Sünnipäev"] = ("День рождения", "Birthday"),
            ["Sünnipäevanädal"] = ("Неделя дня рождения", "Birthday week"),
            ["sünnipäev"] = ("день рождения", "birthday"),
            ["ja"] = ("и", "and"),
            ["torti"] = ("торта", "cake"),
            ["osta"] = ("купи", "buy"),
            ["saa"] = ("получи", "get"),
            ["tasuta"] = ("бесплатно", "free"),
            ["hinnaga"] = ("по цене", "for"),
            ["Osta"] = ("Купи", "Buy"),
            ["tk"] = ("шт", "pcs"),
            ["kg"] = ("кг", "kg"),
        };

        // Täpne tõlge: tuntud sõna/fraas või "<toode> <maht> L/ml/tk"; muidu null
        public static string Tapne(string et)
        {
            if (Praegune == "et" || string.IsNullOrEmpty(et)) return null;
            if (S.TryGetValue(et, out var t) || P.TryGetValue(et, out t)) return Praegune == "ru" ? t.ru : t.en;
            if (Praegune == "ru" && (et.EndsWith(" / kg") || et.EndsWith("/kg"))) return et.Substring(0, et.Length - 2) + "кг";
            var m = Regex.Match(et, @"^(.+?) (\d+(?:,\d+)?) (L|ml|tk)$");
            if (m.Success)
            {
                string alus = m.Groups[1].Value;
                string tb = (S.TryGetValue(alus, out var a) || P.TryGetValue(alus, out a)) ? (Praegune == "ru" ? a.ru : a.en) : alus;
                string u = m.Groups[3].Value;
                string uu = Praegune == "ru" ? (u == "L" ? "л" : u == "ml" ? "мл" : "шт") : (u == "tk" ? "pcs" : u);
                return tb + " " + m.Groups[2].Value + " " + uu;
            }
            return null;
        }

        // Ühikud kogusetekstis ("2 tk", "0,350 kg", "250 g")
        public static string Uhikud(string s)
        {
            if (Praegune == "et" || string.IsNullOrEmpty(s)) return s;
            if (Praegune == "ru") return s.Replace(" tk", " шт").Replace(" kg", " кг").Replace("/kg", "/кг").Replace(" g", " г");
            return s.Replace(" tk", " pcs");
        }

        // Tõlgib eestikeelse teksti valitud keelde (tundmatu tekst jääb samaks)
        public static string T(string et)
        {
            if (string.IsNullOrEmpty(et) || Praegune == "et") return et;

            if (S.TryGetValue(et, out var t)) return Praegune == "ru" ? t.ru : t.en;

            var tp = Tapne(et);
            if (tp != null) return tp;
            if (Praegune == "ru" && et.EndsWith("/kg")) return et.Substring(0, et.Length - 2) + "кг";

            // "Tere, nimi!   Roll: roll"
            var m = Regex.Match(et, @"^Tere, (.*)!   Roll: (.*)$");
            if (m.Success)
                return Praegune == "ru" ? $"Здравствуйте, {m.Groups[1].Value}!   Роль: {m.Groups[2].Value}"
                                        : $"Hello, {m.Groups[1].Value}!   Role: {m.Groups[2].Value}";

            // tekst, mis algab tõlgitava eesliitega ("Kokku: 12.00 €", "Andmebaasi viga: ...")
            foreach (var kv in S)
            {
                string k = kv.Key;
                if ((k.EndsWith(": ") || k.EndsWith(":\n")) && et.StartsWith(k))
                    return (Praegune == "ru" ? kv.Value.ru : kv.Value.en) + et.Substring(k.Length);
            }
            return et;
        }

        // ---- vormi tõlkimine ----
        private class Hoidja { public string Algne; public string Viimane; }
        private static readonly ConditionalWeakTable<object, Hoidja> Tekstid = new ConditionalWeakTable<object, Hoidja>();
        private static readonly ConditionalWeakTable<object, Hoidja> Vihjed = new ConditionalWeakTable<object, Hoidja>();
        private static readonly ConditionalWeakTable<DataGridView, object> Gridid = new ConditionalWeakTable<DataGridView, object>();
        private static readonly ConditionalWeakTable<ComboBox, object> Kombod = new ConditionalWeakTable<ComboBox, object>();

        private static string Tolgi(object voti, string praegu, ConditionalWeakTable<object, Hoidja> tabel)
        {
            var h = tabel.GetOrCreateValue(voti);
            if (h.Algne == null || praegu != h.Viimane) h.Algne = praegu;   // tekst muutus väljastpoolt: see on uus algtekst
            h.Viimane = T(h.Algne);
            return h.Viimane;
        }

        public static void Rakenda(Form vorm)
        {
            if (!string.IsNullOrEmpty(vorm.Text)) vorm.Text = Tolgi(vorm, vorm.Text, Tekstid);
            RakendaKontrollidele(vorm);
        }

        private static void RakendaKontrollidele(Control juur)
        {
            foreach (Control c in juur.Controls)
            {
                if ((c is Button || c is Label || c is CheckBox || c is RadioButton || c is GroupBox || c is TabPage)
                    && !string.IsNullOrEmpty(c.Text) && c.Text.Any(char.IsLetter))
                    c.Text = Tolgi(c, c.Text, Tekstid);

                if (c is TextBox tb && !string.IsNullOrEmpty(tb.PlaceholderText))
                    tb.PlaceholderText = Tolgi(tb, tb.PlaceholderText, Vihjed);

                if (c is DataGridView g)
                {
                    TolgiPaised(g);
                    if (!Gridid.TryGetValue(g, out _))
                    {
                        Gridid.Add(g, new object());
                        g.DataBindingComplete += (s, e) => TolgiPaised(g);
                        g.Paint += (s, e) => TolgiPaised(g);   // päised, mis pannakse paika pärast andmete sidumist
                        g.CellFormatting += (s, e) =>
                        {
                            if (e.Value is string v && v.Length > 0)
                            {
                                var x = Tapne(v);
                                if (x != null) { e.Value = x; e.FormattingApplied = true; }
                            }
                        };
                    }
                }
                if (c is ComboBox cb && !(cb.Tag as string == "keel") && !Kombod.TryGetValue(cb, out _))
                {
                    Kombod.Add(cb, new object());
                    cb.FormattingEnabled = true;
                    cb.Format += (s, e) =>
                    {
                        if (e.Value is string v) { var x = Tapne(v); if (x != null) e.Value = x; }
                    };
                    // juba lisatud ripploendi read tuleb uuesti vormistada (muidu jääb vana tekst); ainult ühe korra
                    if (cb.Items.Count > 0)
                        typeof(ComboBox).GetMethod("RefreshItems", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(cb, null);
                }
                RakendaKontrollidele(c);
            }
        }

        private static void TolgiPaised(DataGridView g)
        {
            foreach (DataGridViewColumn col in g.Columns)
                if (!string.IsNullOrEmpty(col.HeaderText))
                {
                    string uus = Tolgi(col, col.HeaderText, Tekstid);
                    if (uus != col.HeaderText) col.HeaderText = uus;
                }
        }

        // Keelevalik (ripploend) vormi paremasse ülanurka; vahetamisel tõlgitakse vorm kohe
        public static ComboBox Valik(Form vorm, Action peale = null)
        {
            var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 105, Tag = "keel" };
            cb.Items.AddRange(new object[] { "Eesti", "Русский", "English" });
            cb.SelectedIndex = Praegune == "ru" ? 1 : Praegune == "en" ? 2 : 0;
            cb.SelectedIndexChanged += (s, e) =>
            {
                Sea(cb.SelectedIndex == 1 ? "ru" : cb.SelectedIndex == 2 ? "en" : "et");
                Rakenda(vorm);
                peale?.Invoke();
            };
            return cb;
        }
    }

    // Asendab System.Windows.Forms.MessageBox'i: teated tõlgitakse valitud keelde
    public static class MessageBox
    {
        public static DialogResult Show(string tekst) =>
            System.Windows.Forms.MessageBox.Show(Keel.T(tekst), Keel.T("Teade"));

        public static DialogResult Show(string tekst, string pealkiri) =>
            System.Windows.Forms.MessageBox.Show(Keel.T(tekst), Keel.T(pealkiri));

        public static DialogResult Show(string tekst, string pealkiri, MessageBoxButtons nupud) =>
            System.Windows.Forms.MessageBox.Show(Keel.T(tekst), Keel.T(pealkiri), nupud);

        public static DialogResult Show(string tekst, string pealkiri, MessageBoxButtons nupud, MessageBoxIcon ikoon) =>
            System.Windows.Forms.MessageBox.Show(Keel.T(tekst), Keel.T(pealkiri), nupud, ikoon);
    }
}
