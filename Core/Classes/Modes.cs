using System;

public static class Modes
{
    public const string GetPrih = "-GetPrih";
    public const string PrihAnswer = "-PrihAnswer";
    public const string GetOrder = "-GetOrder";
    public const string OrderAnswer = "-OrderAnswer";
    public const string OrderAnswerOpt = "-OrderAnswerOpt";
    public const string Rez = "-Rez";
    public const string SnRez = "-SnRez";
    public const string Vzvr = "-Vzvr";
    public const string Def = "-Def";

    public const string Ostatki = "-Ostatki";    
    public const string AdresDelivery = "-AdresDelivery";    
    public const string HandPrihMd = "-HandPrihMd";
    public const string HandOrderMd = "-HandOrderMd";

    public const string Test = "-Test";

    public static bool CheckMode(string s)
    {
        try
        {
            if (String.IsNullOrEmpty(s))
                return false;
            foreach (var prop in typeof(Modes).GetFields())
                if (prop.GetValue(prop).ToString() == s)
                    return true;
            return false;
        }
        catch { return false; }
    }

    public static void PrintActualModes()
    {
        Console.WriteLine("Список актуальных параметров:");
        Console.WriteLine("   {0}  \t= {1}", GetPrih, "Загрузка приходов");
        Console.WriteLine("   {0} \t= {1}", PrihAnswer, "Отправка ответов на приходы");
        Console.WriteLine("   {0} \t= {1}", Ostatki, "Формирование остатков (прайс-листов)");
        Console.WriteLine("   {0} \t= {1}", GetOrder, "Загрузка заказов");
        Console.WriteLine("   {0} \t= {1}", OrderAnswer, "Отправка ответов на заказы");
        Console.WriteLine("   {0} \t= {1}", OrderAnswerOpt, "Отправка ответов на оптовые отгрузки");
        Console.WriteLine("   {0} \t= {1}", Vzvr, "Отправка возвратов ВнПс");
        Console.WriteLine("   {0} \t= {1}", Def, "Отправка дефектуры Котельников");
        Console.WriteLine("   {0} \t= {1}", AdresDelivery, "Отправка файла Adres_delivery.dbf");
        Console.WriteLine("   {0} \t= {1}", Rez, "Загрузка резервов");
        Console.WriteLine("   {0} \t= {1}", SnRez, "Снятие резервов");
        Console.WriteLine("   {0} \t= {1}", HandPrihMd, "Выгрузка MD файла ПРИХОДА для МДЛП (ручная)");
        Console.WriteLine("   {0} \t= {1}", HandOrderMd, "Выгрузка MD файла ЗАКАЗА для МДЛП (ручная)");
        //Console.WriteLine("   {0} \t= {1}", SendDoc, "Выгрузка накладных");
        
        Console.WriteLine("(!) Программа может быть вызвана только с одним параметром. Остальные параметры игнорируются.");
    }
}