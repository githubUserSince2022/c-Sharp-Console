using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
//namespace TstUI;
static class Program
{
    private enum Verfahren{
        Auszahlung, Einzahlung, Bargeldeingabe
    }
    private static float bargeld; 
    private static Verfahren _verfahren;
    private static float kontostand = 0;
    private static bool abfrage=true;
    private static bool fehler=true;
    private static string vorname;
    private static string nachname;
    public static void Main(string[] args)
    {
        // System.Windows.Forms.Form mainForm = new Form();
        // Label lblFirst = new Label();
        // mainForm.Width = 300;
        // mainForm.Height = 400; 
        // lblFirst.Text = "Hello World";
        // lblFirst.Location = new Point(150,200);
        // mainForm.Controls.Add(lblFirst);
        // Application.Run(mainForm);
        Console.WriteLine("");
        Console.WriteLine("Herzlich Willkommen bei deiner neuen Bank!! \n");

        while(fehler){
            Console.WriteLine("Gib deinen Vornamen an \n");
            vorname = Console.ReadLine();
            try{
                Console.WriteLine("");
                if(checkName(vorname)){
                    fehler = false;
                }
            }
            catch(Exception ex){
                Console.WriteLine(ex);
            }
        }
        fehler = true;
        while(fehler){
            Console.WriteLine("Gib deinen Nachnamen an \n");
            nachname = Console.ReadLine();
            try{
                Console.WriteLine("");
                if(checkName(nachname)){
                    fehler = false;
                }
            }
            catch(Exception ex){
                Console.WriteLine(ex);
            }
        }
        Console.WriteLine("\nGebe deinen momentanen Geldbetrag an, den du in bar hast ");
        fehler = true;
        while(fehler){
            try{
                bargeld = float.Parse(Console.ReadLine());
                fehler = false;
                _verfahren = Verfahren.Bargeldeingabe;
                // writeJSON();
                // readJSON();
            }
            catch(Exception ex){
                Console.WriteLine("\nGib bitte eine gültige Zahl für dein aktuelles Bargeldvermögen an!!");
            }
        }
        Console.WriteLine("");
        Console.WriteLine("Du hast " + bargeld + " Euro eingegeben \n");

        try{
            connectToDB();
        }
        catch(Exception ex){
            Console.WriteLine(ex);
        }

        while(abfrage){
             Thread.Sleep(1000);
             Console.WriteLine("Wenn du Geld abheben willst, dann schreibe das Wort a");
             Thread.Sleep(1000);
             Console.WriteLine("Wenn du Geld einzahlen willst, dann schreibe das Wort e");
             Thread.Sleep(1000);
             Console.WriteLine("Wenn du den Vorgang beenden willst, dann schreibe das Wort b \n");

             string eingabe= Console.ReadLine();
             if(eingabe.ToUpper()=="A"){
                try{
                    abheben();
                }
                catch(Exception ex){ Console.WriteLine(ex);}
             }
             else if(eingabe.ToUpper()=="E"){
                try{
                    einzahlen();
                }
                catch(Exception ex){ Console.WriteLine(ex);}
             }
             else if(eingabe.ToUpper()=="B"){
                Console.WriteLine("\nVorgang beendet");
                abfrage= false;
             }
             else{
                Console.WriteLine("\nBefehl nicht erkannt!!");
             }
             Console.WriteLine("\n\n");
        }
        
    }
    public static void abheben(){
        Console.WriteLine("\nWie viel Geld möchtest du abheben ? ");
        float abheben = float.Parse(Console.ReadLine());
        Console.WriteLine("");
        if(kontostand>0 && (kontostand-abheben>=0) && abheben>0){
            kontostand -= abheben;
            bargeld += abheben;
            Thread.Sleep(1000);
            Console.WriteLine("\nEs wurden " + abheben + " Euro abgehoben");
            Thread.Sleep(1000);
            //ausgabeVermögen();
            _verfahren= Verfahren.Auszahlung;
            // writeJSON();
            // readJSON();
            insertInDB();
        }   
        else if(abheben<=0){
            Console.WriteLine("Bitte eine positive Zahl größer 0 angeben!!");
        }
        else {
            Console.WriteLine("Du möchtest " + abheben + " Euro abheben, hast aber " + kontostand + " Euro auf dem Konto");
        }
    }
    public static void einzahlen(){
        Console.WriteLine("\nWie viel Geld möchtest du einzahlen ? ");
        float einzahlen = float.Parse(Console.ReadLine());
        Console.WriteLine("");
        if(einzahlen<=bargeld && einzahlen>0){
            kontostand+= einzahlen;
            bargeld-= einzahlen;
            Thread.Sleep(1000);
            Console.WriteLine("\nEs wurden " + einzahlen + " Euro eingezahlt");
            Thread.Sleep(1000);
            _verfahren= Verfahren.Einzahlung;
            // writeJSON();
            // readJSON();
            insertInDB();
        }
        else if(einzahlen<=0){
            Console.WriteLine("Bitte eine positive Zahl größer 0 angeben!!");
        }
        else{
            Console.WriteLine("Du möchtest " + einzahlen + " Euro einzahlen, hast aber nur " + bargeld + " Euro Bargeld");
        }
    }
    public static void ausgabeVermögen(){
        Console.WriteLine("\nIhr Kontostand beträgt " + kontostand + " Euro");
        Console.WriteLine("Ihr Bargeldvermögen beträgt " + bargeld + " Euro");
    }
    private static int i=0;
    public class Data{
        public int ID { get; set; }
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public float Bargeld { get; set; }
        public float Kontostand { get; set; }
        public DateTime Zeit {get;set;}
        public String Verfahren {get;set;}
        
        public override string ToString(){
            Console.OutputEncoding = System.Text.Encoding.Default;
            return "ID: " + ID + " Vorname: " + Vorname  + " Bargeld: " + Bargeld + "€" + " Kontostand: " + Kontostand + "€" + " Datum: " + Zeit + " Durch: " + Verfahren;
        }
    }
    public static List<Data> _data = new List<Data>();
    public static void writeJSON(){
        try{
            i++;
            _data.Add(new Data()
            {
                ID = i,
                Vorname = vorname,
                Nachname= nachname,
                Bargeld = bargeld,
                Kontostand = kontostand,
                Zeit = DateTime.Now,
                Verfahren = _verfahren.ToString()
            });
            string json = JsonSerializer.Serialize(_data);
            File.WriteAllText("Hallo.json", json);
        }
        catch(Exception ex){
            Console.WriteLine(ex);
        }   
    }
    public static void readJSON(){
        try{
            using (StreamReader r = new StreamReader("Hallo.json"))
            {
                string json = r.ReadToEnd();
                List<Data> items =  Newtonsoft.Json.JsonConvert.DeserializeObject<List<Data>>(json);
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                for(int i=items.Count-1;i>=0;i--){
                    Console.WriteLine(items[i]);
                }
                //items.ForEach(Console.WriteLine);
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            }
        }
        catch(Exception ex){
            Console.WriteLine(ex);
        }
    }
    public static bool checkName(string name){
        if(name==vorname){
            string source = @"all.txt";
            var names = new HashSet<string>();
            using (var sr = new StreamReader(source))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    names.Add(line.ToLower());
                }
            }
            var found = false;
            if (names.Contains(name.ToLower()))
            {
                Console.WriteLine("Name vermerkt");
                found = true;
            }
            return found;
        }
        else if(name==nachname){
            return Regex.IsMatch(nachname, @"^[a-zA-Z]+$");
        }
        return false;  
    }
    public static string connectionString="SERVER=" + "localhost" + ";" + "DATABASE=" + "bank" + ";" + "UID=" + "root" + ";" + "PASSWORD=" + "" + ";";
    public static MySqlConnection connection;
    public static void connectToDB(){
        try{
            connection = new MySqlConnection(connectionString);
            connection.Open();
            //deleteRowsinDB();
            insertInDB();
        }
        catch(Exception ex){
            Console.WriteLine(ex);
        }
    }
    // Benutze ich, nachdem ich eine Zeile gelöscht habe, um Autoincrement zurückzusetzen
    private static void deleteRowsinDB(){
        string query2="Truncate table daten;";
        MySqlCommand cmd2 = new MySqlCommand(query2, connection);
        cmd2.ExecuteNonQuery();
    }
    private static void insertInDB(){ 
        try{
            DateTime datum = DateTime.Now;
            string query= "INSERT INTO DATEN(Vorname,Nachname,Bargeld,Kontostand,Datum,Verfahren) VALUES (@Vorname,@Nachname,@Bargeld,@Kontostand,@Datum,@Verfahren);";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Vorname", vorname);
            cmd.Parameters.AddWithValue("@Nachname", nachname);
            cmd.Parameters.AddWithValue("@Bargeld", bargeld);
            cmd.Parameters.AddWithValue("@Kontostand", kontostand);
            cmd.Parameters.AddWithValue("@Datum", datum);
            cmd.Parameters.AddWithValue("@Verfahren", _verfahren.ToString());
            cmd.ExecuteNonQuery();
            getDataFromDB().ForEach(Console.WriteLine);
            Console.WriteLine("");
        }
        catch (MySqlException ex)
        {
            switch (ex.Number)
            {
                case 0:
                    Console.WriteLine("Cannot connect to server.  Contact administrator");
                    break;

                case 1045:
                    Console.WriteLine("Invalid username/password, please try again");
                    break;
            }
            Console.WriteLine(ex); 
        }
    }
    public static List<Data> getData = new();
    public static List<Data> getDataFromDB(){
        try{
            string query = "Select Id,Vorname,Nachname,Bargeld,Kontostand,Datum,Verfahren from DATEN";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read()){
                getData.Add(new Data(){
                    ID = (int)reader["Id"],
                    Vorname = reader["Vorname"].ToString(),
                    Nachname= reader["Nachname"].ToString(),
                    Bargeld = (float)reader["Bargeld"],
                    Kontostand = (float)reader["Kontostand"],
                    Zeit = (DateTime)reader["Datum"],
                    Verfahren = reader["Verfahren"].ToString()
                });     
            }
            reader.Close();
        }
        catch(Exception ex){
            Console.WriteLine(ex);
        }
        return getData;
    }
}

// Neben Datum auch schreiben ob Einzahlung oder Auszahlung(statt ID wahrscheinlich)
// alles nochmal laden für einen bestimmten User mit Login am besten
// Ein Admin der alles sehen kann
