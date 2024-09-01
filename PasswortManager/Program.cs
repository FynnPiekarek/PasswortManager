using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using static Program;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Runtime.Serialization;

//Programm:
class Program
{
    static List<Person> people = new List<Person>();
    public List<Password> Passwords { get; } = new List<Password>();
    static int nextPersonID = 1;
    static int nextPasswordListID = 1;
    static Person loggedInPerson;
    private static int i;


    // Main Funktion mit eingabe zu switch case.
    static void Main()
    {
        string peopleFilePath = "C:\\Users\\fynnp\\OneDrive - sluz\\Manager\\manager_people_data.json";
        string passwordsFilePath = "C:\\Users\\fynnp\\OneDrive - sluz\\Manager\\manager_passwords_data.json";

        if (File.Exists(peopleFilePath))
        {
            string peopleJson = File.ReadAllText(peopleFilePath);

            if (!string.IsNullOrEmpty(peopleJson))
            {
                people = LoadData<Person>(peopleFilePath);
            }
        }


        Start();

        if (File.Exists(passwordsFilePath))
        {
            string passwordsJson = File.ReadAllText(passwordsFilePath);

            if (!string.IsNullOrEmpty(passwordsJson))
            {
                loggedInPerson.Passwords = LoadData<Password>(passwordsFilePath);
            }
        }
        while (true)
        {
            Console.Clear();
            int top = Console.CursorTop; // Zeilenposition
            var option = 1; //Setzt decorator auf 1
            var decorator = "> ";

            ConsoleKeyInfo key;
            bool isSelected = false; //Setzt fest das noch keine Funktion ausgewehlt wurde

            while (!isSelected)
            {
                Console.CursorVisible = false; //Verbirgt den Cursor
                Console.SetCursorPosition(0, top); //Setzt die Curser Position um mit weiteren eingaben den bestehenden Text zu überschreiben

                DisplayBanner();
                //Funktions auswahl + decorator
                Console.WriteLine("   Befehle:");
                Console.WriteLine($"{(option == 1 ? decorator : "   ")}1: Passwort hinzufügen ");
                Console.WriteLine($"{(option == 2 ? decorator : "   ")}2: Passwort generieren ");
                Console.WriteLine($"{(option == 3 ? decorator : "   ")}3: Passwörter Auflisten ");
                Console.WriteLine($"{(option == 4 ? decorator : "   ")}4: Passwort Suchen ");
                Console.WriteLine($"{(option == 5 ? decorator : "   ")}5: Konto ");
                Console.WriteLine($"{(option == 6 ? decorator : "   ")}6: Beenden ");
                Console.WriteLine("  ============================================================");


                key = Console.ReadKey(false); //Schaut ob einer der zwei Arrow Keys gedrückt wird

                switch (key.Key) //Bei drücken der Pfeiltasten wird der decorator verschoben und die Option geändert. Beim drücken der Enter Taste wird die jetzige Option ausgewählt
                {
                    case ConsoleKey.UpArrow:
                        option = option == 1 ? 6 : option - 1;
                        break;

                    case ConsoleKey.DownArrow:
                        option = option == 6 ? 1 : option + 1;
                        break;

                    case ConsoleKey.Enter:
                        isSelected = true;
                        break;
                }
            }

            switch (option) //Switch case für 6 verschiedene Optionen und einer default Option
            {
                case 1:
                    Console.Clear();//Console wird geleert
                    DisplayBanner();//Standart Displaybanner
                    do//do while Schlaufe welche das Programm so lange wiederholt wie man will 
                    {
                        Console.CursorVisible = true;
                        AddPassword();//Bestimmte Funktion welche Ausgeführt wird
                    } while (AskForRepeat());
                    Console.Clear();//Console wird geleert
                    break;//Springt aus dem Case wieder in die Main Methode
                case 2:
                    Console.Clear();
                    DisplayBanner();
                    do
                    {
                        Console.CursorVisible = true;
                        generatePassword();
                    } while (AskForRepeat());
                    Console.Clear();
                    break;
                case 3:
                    Console.Clear();
                    DisplayBanner();
                    ListPasswords(loggedInPerson);
                    Console.Clear();
                    break;
                case 4:
                    Console.Clear();
                    DisplayBanner();
                    do
                    {
                        SearchPasswords();
                    } while (AskForRepeat());
                    Console.Clear();
                    break;
                case 5:
                    Console.Clear();
                    DisplayBanner();
                    Console.CursorVisible = true;
                    AccountInfo();

                    Console.Clear();
                    break;
                case 6:
                    // Speichere die Daten der 'people'-Liste
                    SaveData(people, peopleFilePath);

                    // Speichere die Daten der 'Passwords'-Liste
                    SaveData(loggedInPerson.Passwords, passwordsFilePath);
                    Environment.Exit(0);
                    break;
                default: //Standart Ausgabe bei falscher eingabe (wird nichtmehr verwendet durch das auswählen mit Pfeiltasten)
                    Console.WriteLine("  Ungültige Eingabe. Bitte wählen Sie eine der verfügbaren Optionen.");
                    Console.ReadKey();
                    break;
            }
        }
    }
    static void Start()
    {
        DisplayBanner();
        if (people.Count == 0)
        {
            CreateAccount();
            return;
        }

        Console.WriteLine("  1. Anmelden");
        Console.WriteLine("  2. Account erstellen");
        Console.WriteLine("  3. Beenden");
        Console.Write("  Wähle eine Option: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Login();
                break;
            case "2":
                CreateAccount();
                break;
            case "3":
                string peopleFilePath = "C:\\Users\\fynnp\\OneDrive - sluz\\Manager\\manager_people_data.json";
                string passwordsFilePath = "C:\\Users\\fynnp\\OneDrive - sluz\\Manager\\manager_passwords_data.json";

                // Speichere die Daten der 'people'-Liste
                SaveData(people, peopleFilePath);

                // Speichere die Daten der 'Passwords'-Liste
                SaveData(loggedInPerson.Passwords, passwordsFilePath);
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("  Ungültige Option. Bitte erneut versuchen.");
                break;
        }
    }
    static void Login()
    {
        Console.Write("  Benutzername (E-Mail): ");
        string email = Console.ReadLine().ToLower();
        Console.Write("  Passwort: ");
        string loginpassword = GetPassword();
        foreach (var person in people)
        {
            if (person.Mail == email && person.LoginPasswort == loginpassword)
            {
                var twofaresp = twofa(email);
                if (twofaresp == true)
                {
                    loggedInPerson = person;
                    Console.WriteLine($"  Erfolgreich eingeloggt! Deine PersonID ist {person.PersonID}");
                    return;
                }
                return;
            }
        }

        Console.WriteLine("  Ungültige Anmeldeinformationen. Bitte erneut versuchen.");
        Console.WriteLine("  Möchtest du dein Passwort zurücksetzen? (ja/nein)");
        string response = Console.ReadLine().ToLower();
        if (response == "ja")
        {
            ResetPassword();
        }
    }
    static void Logout()
    {
        loggedInPerson = null;
        Console.WriteLine("  Sie wurden erfolgreich abgemeldet.");
        Console.WriteLine("  Drücken Sie eine beliebige Taste, um fortzufahren...");
        Console.ReadKey();
        Console.Clear();
        Start();
    }
    static void CreateAccount()
    {
        Console.WriteLine("  Account erstellen");
        Console.Write("  Name: ");
        string name = Console.ReadLine();
        Console.Write("  E-Mail: ");
        string email = Console.ReadLine().ToLower();

        // Überprüfung der E-Mail-Adresse
        while (!IsValidEmail(email))
        {
            Console.WriteLine("  Die eingegebene E-Mail-Adresse ist ungültig. Bitte geben Sie eine gültige E-Mail-Adresse ein.");
            Console.Write("  E-Mail: ");
            email = Console.ReadLine();
        }

        Console.Write("  Passwort: ");
        string loginpassword = GetPassword();

        // Überprüfung des Passworts
        while (!IsStrongPassword(loginpassword))
        {
            Console.WriteLine("  Das Passwort erfüllt nicht die Mindestanforderungen.");
            Console.WriteLine("  Es muss mindestens 8 Zeichen lang sein und mindestens einen Großbuchstaben, einen Kleinbuchstaben, eine Ziffer und ein Sonderzeichen enthalten.");
            Console.Write("  Bitte geben Sie ein neues Passwort ein: ");
            loginpassword = GetPassword();
        }


        DateTime creationDate = DateTime.Now; // Aktuelles Datum und Uhrzeit

        var person = new Person($"p{nextPersonID}", name, email, loginpassword, $"pl{nextPasswordListID}");
        person.CreationDate = creationDate; // Setze das Erstellungsdatum
        people.Add(person);
        nextPersonID++;
        nextPasswordListID++;
        var twofaresp = twofa(email);
        Console.WriteLine("  Wir haben Ihnen einen Bestätigungs-Code an Ihre Email gesendet, bestätigen Sie diesen.");
        if (twofaresp == true)
        {
            Console.WriteLine($"  Account erstellt! Deine PersonID ist {person.PersonID}");
            loggedInPerson = person;
            return;
        }
        Environment.Exit(0);
    }



    // Funktion zur Überprüfung einer gültigen E-Mail-Adresse mit regulären Ausdrücken
    static bool IsValidEmail(string email)
    {
        // Einfacher regulärer Ausdruck zur Überprüfung von E-Mail-Adressen
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, emailPattern);
    }



    //Funktion welche abfragt ob man den vorgang wiederholen möchte
    static bool AskForRepeat()
    {
        Console.Write("  Möchten Sie die gleiche Funktion erneut aufrufen? (ja/nein): ");
        string response = Console.ReadLine().ToLower(); //ToLower für eine richtige eingabe bein ja/Ja/jA/JA, mann muss nicht umbedingt nein eingeben
        Console.WriteLine();
        Console.WriteLine("  ============================================================");
        return response == "ja";
    }
    //Der verwendete Display Banner als Funktion für einfache wiederverwendung von der seite:https://patorjk.com/software/taag/
    static void DisplayBanner()
    {
        Console.WriteLine("      __  ______    _   _____   ________________ ");
        Console.WriteLine("     /  |/  /   |  / | / /   | / ____/ ____/ __ \\");
        Console.WriteLine("    / /|_/ / /| | /  |/ / /| |/ / __/ __/ / /_/ /");
        Console.WriteLine("   / /  / / ___ |/ /|  / ___ / /_/ / /___/ _, _/ ");
        Console.WriteLine("  /_/  /_/_/  |_/_/ |_/_/  |_\\____/_____/_/ |_|  ");
        Console.WriteLine("  ============================================================");
    }


    private static readonly string aesKey = "2b7e151628aed2a6abf7158809cf4f3c";

    public static (string EncryptedData, byte[] IV) Encrypt<T>(List<T> data)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(aesKey);
            aesAlg.GenerateIV();
            byte[] iv = aesAlg.IV;

            string jsonData = JsonConvert.SerializeObject(data);

            byte[] encryptedData;
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(jsonData);
                }
                encryptedData = msEncrypt.ToArray();
            }

            return (Convert.ToBase64String(encryptedData), iv);
        }
    }


    public static List<T> Decrypt<T>(string encryptedData, byte[] key, byte[] iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedData)))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                string jsonData = srDecrypt.ReadToEnd();
                return JsonConvert.DeserializeObject<List<T>>(jsonData);
            }
        }
    }

    public static void SaveData<T>(List<T> data, string filePath)
    {
        (string encryptedData, byte[] iv) = Encrypt(data);

        var dataToSave = new
        {
            EncryptedData = encryptedData,
            IV = Convert.ToBase64String(iv)
        };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(dataToSave));
    }
    public static List<T> LoadData<T>(string filePath)
    {
        var dataFromJson = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(filePath));
        string encryptedData = dataFromJson.EncryptedData;

        string ivBase64 = dataFromJson.IV; // IV-String aus der JSON-Datei

        // Base64-Decodieren des IV-Strings
        byte[] iv = Convert.FromBase64String(ivBase64);

        return Decrypt<T>(encryptedData, Encoding.UTF8.GetBytes(aesKey), iv);
    }




    static void AddPassword()
    {
        Console.Write("  Beschreibung: ");
        string description = Console.ReadLine();

        // Überprüfung, ob eine Beschreibung bereits existiert
        while (loggedInPerson.Passwords.Any(p => p.Description == description))
        {
            Console.WriteLine("  Es existiert bereits ein Passwort mit dieser Beschreibung.");
            Console.Write("  Bitte geben Sie eine neue Beschreibung ein: ");
            description = Console.ReadLine();
        }

        Console.Write("  Passwort: ");
        string key = GetPassword();

        // Überprüfung des Passworts
        while (!IsStrongPassword(key))
        {
            Console.WriteLine("  Das Passwort erfüllt nicht die Mindestanforderungen.");
            Console.WriteLine("  Es muss mindestens 8 Zeichen lang sein und mindestens einen Großbuchstaben, einen Kleinbuchstaben, eine Ziffer und ein Sonderzeichen enthalten.");
            Console.Write("  Bitte geben Sie ein neues Passwort ein: ");
            key = GetPassword();
        }

        var newPassword = new Password(description, key, loggedInPerson.PasswordListId);
        loggedInPerson.Passwords.Add(newPassword);

        Console.WriteLine("  Passwort hinzugefügt!");
    }

    static bool IsStrongPassword(string password)
    {
        // Mindestlängenprüfung
        if (password.Length < 8)
            return false;

        // Mindestens 1 Zahl, 1 spezielles Zeichen, 1 Kleinbuchstabe und 1 Großbuchstabe
        bool hasDigit = false;
        bool hasSpecialChar = false;
        bool hasLowercase = false;
        bool hasUppercase = false;

        foreach (char c in password)
        {
            if (char.IsDigit(c))
                hasDigit = true;
            else if ("!@#$%^&*-".Contains(c)) // Spezielle Zeichen prüfen
                hasSpecialChar = true;
            else if (char.IsLower(c))
                hasLowercase = true;
            else if (char.IsUpper(c))
                hasUppercase = true;
        }

        // Alle Bedingungen erfüllt?
        return hasDigit && hasSpecialChar && hasLowercase && hasUppercase;
    }
    static void generatePassword()
    {
        string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        string digitChars = "0123456789";
        string specialChars = "!@#$%^&*";

        Console.Write("  Gesamtlänge des Passworts: ");
        int passwordLength = int.Parse(Console.ReadLine());

        Console.Write("  Anzahl der Großbuchstaben: ");
        int uppercaseCount = int.Parse(Console.ReadLine());

        Console.Write("  Anzahl der Kleinbuchstaben: ");
        int lowercaseCount = int.Parse(Console.ReadLine());

        Console.Write("  Anzahl der Zahlen: ");
        int digitCount = int.Parse(Console.ReadLine());

        Console.Write("  Anzahl der Sonderzeichen: ");
        int specialCount = int.Parse(Console.ReadLine());

        int requiredCount = uppercaseCount + lowercaseCount + digitCount + specialCount;

        if (passwordLength < requiredCount || requiredCount < 8)
        {
            Console.WriteLine("  Ungültige Eingabe. Das Passwort muss mindestens 8 Zeichen enthalten und die angegebenen Anforderungen erfüllen.");
            return;
        }

        string allChars = uppercaseChars + lowercaseChars + digitChars + specialChars;

        Random random = new Random();
        string password = "";

        for (int i = 0; i < uppercaseCount; i++)
        {
            password += uppercaseChars[random.Next(uppercaseChars.Length)];
        }

        for (int i = 0; i < lowercaseCount; i++)
        {
            password += lowercaseChars[random.Next(lowercaseChars.Length)];
        }

        for (int i = 0; i < digitCount; i++)
        {
            password += digitChars[random.Next(digitChars.Length)];
        }

        for (int i = 0; i < specialCount; i++)
        {
            password += specialChars[random.Next(specialChars.Length)];
        }

        int remainingLength = passwordLength - requiredCount;

        while (remainingLength > 0)
        {
            int choice = random.Next(4);

            if (choice == 0 && uppercaseCount < passwordLength)
            {
                password += uppercaseChars[random.Next(uppercaseChars.Length)];
                remainingLength--;
            }
            else if (choice == 1 && lowercaseCount < passwordLength)
            {
                password += lowercaseChars[random.Next(lowercaseChars.Length)];
                remainingLength--;
            }
            else if (choice == 2 && digitCount < passwordLength)
            {
                password += digitChars[random.Next(digitChars.Length)];
                remainingLength--;
            }
            else if (choice == 3 && specialCount < passwordLength)
            {
                password += specialChars[random.Next(specialChars.Length)];
                remainingLength--;
            }
        }

        char[] passwordArray = password.ToCharArray();
        // Mische die Zeichen zufällig
        for (int i = 0; i < passwordArray.Length - 1; i++)
        {
            int j = random.Next(i, passwordArray.Length);
            char temp = passwordArray[i];
            passwordArray[i] = passwordArray[j];
            passwordArray[j] = temp;
        }

        password = new string(passwordArray);
        Console.WriteLine("  Generiertes Passwort: " + password);
        Console.Write("  Möchten Sie dieses Passwort hinzufügen? (ja/nein): ");
        string response = Console.ReadLine().ToLower();

        if (response == "ja")
        {
            Console.Write("  Beschreibung : ");
            string description = Console.ReadLine();
            while (loggedInPerson.Passwords.Any(p => p.Description == description))
            {
                Console.WriteLine("  Es existiert bereits ein Passwort mit dieser Beschreibung.");
                Console.Write("  Bitte geben Sie eine neue Beschreibung ein: ");
                description = Console.ReadLine();
            }
            var newPassword = new Password(description, password, loggedInPerson.PasswordListId);
            loggedInPerson.Passwords.Add(newPassword);

            Console.WriteLine("  Passwort hinzugefügt!");
        }
    }

    static void ListPasswords(Person loggedInPerson)
    {
        var sortedPasswords = loggedInPerson.Passwords.OrderBy(p => p.Key).ToList();

        int selectedOption = 1;
        bool isSelected = false;
        var decorator = "> ";
        while (!isSelected)
        {
            Console.Clear();
            DisplayBanner();
            Console.WriteLine("   Liste der Passwörter (alphabetisch sortiert):");

            for (int i = 0; i < sortedPasswords.Count; i++)
            {
                Console.WriteLine($"{(selectedOption == i + 1 ? decorator : "   ")}{i + 1} - {sortedPasswords[i].Description} - {sortedPasswords[i].Key}");
            }
            Console.WriteLine($"{(selectedOption == sortedPasswords.Count + 1 ? decorator : "   ")}Zurück");
            Console.WriteLine("  ============================================================");

            ConsoleKeyInfo key = Console.ReadKey(false);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedOption = selectedOption == 1 ? sortedPasswords.Count + 1 : selectedOption - 1;
                    break;

                case ConsoleKey.DownArrow:
                    selectedOption = selectedOption == sortedPasswords.Count + 1 ? 1 : selectedOption + 1;
                    break;

                case ConsoleKey.Enter:
                    isSelected = true;
                    if (selectedOption == sortedPasswords.Count + 1)
                    {
                        break;
                    }
                    if (selectedOption >= 1 && selectedOption <= sortedPasswords.Count)
                    {
                        // Passwort auswählen und verwalten
                        var selectedPassword = sortedPasswords[selectedOption - 1];
                        ManagePassword(loggedInPerson.Passwords, selectedPassword);
                    }
                    break;
            }
        }
    }


    static void SearchPasswords()
    {
        Console.CursorVisible = true;
        bool isSelected = false;
        string searchQuery = "";
        Console.Write("  Suche (Anfangsbuchstaben): ");
        searchQuery = Console.ReadLine().ToLower(); // Suche in Kleinbuchstaben umwandeln
        Console.CursorVisible = false;
        var matchingPasswords = loggedInPerson.Passwords
            .Where(p => p.Description.ToLower().StartsWith(searchQuery))
            .OrderBy(p => p.Key)
            .ToList();
        var decorator = "> ";
        int selectedOption = 1;
        while (!isSelected)
        {
            Console.Clear();
            DisplayBanner();
            Console.Write($"  Suche (Anfangsbuchstaben): {searchQuery}");
            Console.WriteLine("  Gefundene Passwörter (alphabetisch sortiert):");

            for (int i = 0; i < matchingPasswords.Count; i++)
            {

                Console.WriteLine($"{(selectedOption == i + 1 ? decorator : "   ")}{i + 1} - {matchingPasswords[i].Description} - {matchingPasswords[i].Key}");
            }
            Console.WriteLine($"{(selectedOption == matchingPasswords.Count + 1 ? decorator : "   ")}Zurück");
            Console.WriteLine("  ============================================================");

            ConsoleKeyInfo key = Console.ReadKey(false);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedOption = selectedOption == 1 ? matchingPasswords.Count + 1 : selectedOption - 1;
                    break;

                case ConsoleKey.DownArrow:
                    selectedOption = selectedOption == matchingPasswords.Count + 1 ? 1 : selectedOption + 1;
                    break;

                case ConsoleKey.Enter:
                    isSelected = true;
                    if (selectedOption == matchingPasswords.Count + 1)
                    {
                        break;
                    }
                    else if (selectedOption >= 1 && selectedOption <= matchingPasswords.Count)
                    {
                        // Passwort auswählen und verwalten
                        var selectedPassword = matchingPasswords[selectedOption - 1];
                        ManagePassword(loggedInPerson.Passwords, selectedPassword);
                    }
                    break;
            }
        }
    }



    static void AccountInfo()
    {
        Console.WriteLine("  Kontoinformationen:");
        Console.WriteLine($"  Name: {loggedInPerson.Name}");
        Console.WriteLine($"  E-Mail: {loggedInPerson.Mail}");
        Console.WriteLine($"  Erstellungsdatum: {loggedInPerson.CreationDate}");
        Console.WriteLine($"  Anzahl der gespeicherten Passwörter: {loggedInPerson.Passwords.Count}");
        Console.WriteLine();
        Console.WriteLine("  Befehle:");
        Console.WriteLine("  1: Abmelden");
        Console.WriteLine("  2: Lösche dein Konto");
        Console.WriteLine("  3: Zurück zum Hauptmenü");
        Console.Write("  Wähle eine Option: ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Logout();
                break;
            case "2":
                DeleteAccount();
                break;
            case "3":
                // Zurück zum Hauptmenü
                break;
            default:
                Console.WriteLine("  Ungültige Option. Bitte erneut versuchen.");
                break;
        }
    }
    static void DeleteAccount()
    {
        Console.WriteLine();
        if (loggedInPerson == null)
        {
            Console.WriteLine("  Du musst dich zuerst anmelden, um dein Konto zu löschen.");
            return;
        }

        Console.Write("  Gib dein aktuelles Passwort ein, um dein Konto zu löschen: ");
        string passwordInput = GetPassword();

        if (passwordInput == loggedInPerson.LoginPasswort)
        {
            Console.Write("  Bist du sicher, dass du dein Konto löschen möchtest? (ja/nein): ");
            string response = Console.ReadLine().ToLower();

            if (response == "ja")
            {
                // Lösche alle Passwörter des gelöschten Kontos
                foreach (var person in people)
                {
                    person.Passwords.RemoveAll(p => p.PasswordListId == loggedInPerson.PasswordListId);
                }

                people.Remove(loggedInPerson);
                loggedInPerson = null;
                Console.WriteLine("  Dein Konto wurde gelöscht, und alle zugehörigen Passwörter wurden entfernt.");
                Console.ReadKey();
                // Starte den Programmfluss neu, um zum Anfang des Codes zu gelangen
                Console.Clear();
                Start();
            }
            else
            {
                Console.WriteLine("  Kontolöschung abgebrochen.");
            }
        }
        else
        {
            Console.WriteLine("  Falsches Passwort. Das Konto konnte nicht gelöscht werden.");
        }
    }
    static void ManagePassword(List<Password> passwords, Password selectedPassword)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("  Passwort auswählen:");
            Console.WriteLine($"  Beschreibung: {selectedPassword.Description}");
            Console.WriteLine($"  Passwort: {selectedPassword.Key}");
            Console.WriteLine();
            Console.WriteLine("  Optionen:");
            Console.WriteLine("  1: Beschreibung bearbeiten");
            Console.WriteLine("  2: Passwort bearbeiten");
            Console.WriteLine("  3: Löschen");
            Console.WriteLine("  4: Zurück zur Liste");

            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    Console.Write("  Neue Beschreibung: ");
                    string newDescription = Console.ReadLine();

                    // Überprüfe, ob die neue Beschreibung bereits verwendet wird
                    if (passwords.Any(p => p.Description == newDescription))
                    {
                        Console.WriteLine("  Eine Passwortbeschreibung mit diesem Namen existiert bereits.");
                        Console.WriteLine("  Drücken Sie eine beliebige Taste, um fortzufahren.");
                        Console.ReadKey();
                    }
                    else
                    {
                        selectedPassword.Description = newDescription;
                        Console.WriteLine("  Beschreibung wurde geändert.");
                        Console.WriteLine("  Drücken Sie eine beliebige Taste, um fortzufahren.");
                        Console.ReadKey();
                    }
                    break;

                case "2":
                    Console.Write("  Neues Passwort: ");
                    string newKey = GetPassword();
                    while (!IsStrongPassword(newKey))
                    {
                        Console.WriteLine("  Das Passwort erfüllt nicht die Mindestanforderungen.");
                        Console.WriteLine("  Es muss mindestens 8 Zeichen lang sein und mindestens einen Großbuchstaben, einen Kleinbuchstaben, eine Ziffer und ein Sonderzeichen enthalten.");
                        Console.Write("  Bitte geben Sie ein neues Passwort ein: ");
                        newKey = GetPassword();
                    }
                    selectedPassword.Key = newKey;
                    Console.WriteLine("  Passwort wurde geändert.");
                    Console.WriteLine("  Drücken Sie eine beliebige Taste, um fortzufahren.");
                    Console.ReadKey();
                    break;

                case "3":
                    // Passwort löschen
                    passwords.Remove(selectedPassword);
                    Console.WriteLine("  Passwort wurde gelöscht.");
                    Console.WriteLine("  Drücken Sie eine beliebige Taste, um fortzufahren.");
                    Console.ReadKey();
                    return;

                case "4":
                    // Zurück zur Liste
                    return;

                default:
                    Console.WriteLine("  Ungültige Option. Bitte erneut versuchen.");
                    Console.WriteLine("  Drücken Sie eine beliebige Taste, um fortzufahren.");
                    Console.ReadKey();
                    break;
            }
        }
    }
    static bool twofa(string email)
    {
        Console.WriteLine();
        Random random = new Random();
        int emailkey = random.Next(1000, 10000);
        authenticator(emailkey, email);
        Console.WriteLine("  Ein temporäres Passwort wurde an Ihre E-Mail-Adresse gesendet.");
        Console.Write("  Geben Sie den 4 stelligen Code ein:");
        int enteredKey = Convert.ToInt32(Console.ReadLine());
        if (enteredKey == emailkey)
        {
            return true;
        }
        else
        {
            Console.WriteLine("  Der Code ist leider falsch, versuche es später erneut.");
            Console.ReadKey();
            return false;
        }

    }
    static void ResetPassword()
    {
        Console.WriteLine();
        Random random = new Random();
        int emailkey = random.Next(1000, 10000);
        Console.Write("  Geben Sie Ihre registrierte E-Mail-Adresse ein: ");
        string email = Console.ReadLine().ToLower();

        // Überprüfung, ob die E-Mail-Adresse existiert
        var person = people.FirstOrDefault(p => p.Mail == email);

        if (person != null)
        {
            authenticator(emailkey, email);
            Console.WriteLine("  Ein temporäres Passwort wurde an Ihre E-Mail-Adresse gesendet.");
            Console.Write("  Geben Sie den 4 stelligen Code ein:");
            int enteredKey = Convert.ToInt32(Console.ReadLine());
            if (enteredKey == emailkey)
            {
                Console.Write("  Passwort: ");
                string loginpassword = GetPassword();

                // Überprüfung des Passworts
                while (!IsStrongPassword(loginpassword))
                {
                    Console.WriteLine("  Das Passwort erfüllt nicht die Mindestanforderungen.");
                    Console.WriteLine("  Es muss mindestens 8 Zeichen lang sein und mindestens einen Großbuchstaben, einen Kleinbuchstaben, eine Ziffer und ein Sonderzeichen enthalten.");
                    Console.Write("  Bitte geben Sie ein neues Passwort ein: ");
                    loginpassword = GetPassword();
                }
                loggedInPerson = person;
            }
            else
            {
                Console.WriteLine("  Der Code ist leider falsch, versuche es später erneut.");
            }
        }
        else
        {
            Console.WriteLine("  Die eingegebene E-Mail-Adresse wurde nicht gefunden.");
        }
        Console.ReadKey();
    }

    static void authenticator(int emailkey, string email)
    {
        // GMX-E-Mail-Einstellungen
        string gmxSmtpServer = "mail.gmx.com";
        int gmxSmtpPort = 587; // GMX SMTP-Port
        string gmxEmail = "bbzw_authenticator@gmx.ch";
        string gmxPassword = "bbzw_authenticator123";

        // Empfängerinformationen
        string toEmail = email;
        string subject = "2fa Authenticator";
        string body = $"" +
            $"This is your verification code: {emailkey}";

        // Erstellen der SmtpClient-Instanz
        SmtpClient smtpClient = new SmtpClient(gmxSmtpServer, gmxSmtpPort);
        smtpClient.EnableSsl = true; // Aktiviert SSL-Verschlüsselung

        // Anmelden beim GMX-Konto
        smtpClient.Credentials = new NetworkCredential(gmxEmail, gmxPassword);

        // Erstellen der E-Mail-Nachricht
        MailMessage mail = new MailMessage(gmxEmail, toEmail);
        mail.Subject = subject;
        mail.Body = body;

        try
        {
            // Senden der E-Mail
            smtpClient.Send(mail);
            Console.WriteLine("  E-Mail erfolgreich versendet.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("  Fehler beim Versenden der E-Mail: " + ex.Message);
        }
        finally
        {
            // Aufräumen
            mail.Dispose();
            smtpClient.Dispose();
        }
    }
    static string GetPassword()
    {
        string getpassword = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(intercept: true);

            // Überprüfen, ob eine Zeichenkombination beendet wurde (Enter-Taste)
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine(); // Neue Zeile für das visuelle Feedback
                break;
            }
            else if (key.Key == ConsoleKey.Backspace && getpassword.Length > 0)
            {
                // Wenn Backspace gedrückt wird, entfernen Sie das letzte Zeichen aus dem Passwort
                getpassword = getpassword.Substring(0, getpassword.Length - 1);
                Console.Write("\b \b"); // Löschen Sie das Zeichen in der Konsole
            }
            else if (!char.IsControl(key.KeyChar)) // Stellen Sie sicher, dass es sich um ein druckbares Zeichen handelt
            {
                getpassword += key.KeyChar;
                Console.Write("*"); // Anzeigen von "*" anstelle des eingegebenen Zeichens
            }
        } while (true);

        return getpassword;
    }



    public class Person
    {
        public string PersonID { get; }
        public string Name { get; }
        public string Mail { get; }
        public string LoginPasswort { get; }
        public string PasswordListId { get; }
        public DateTime CreationDate { get; set; }
        public List<Password> Passwords { get; set; } = new List<Password>();

        public Person(string personID, string name, string mail, string loginpasswort, string passwordlistId)
        {
            PersonID = personID;
            Name = name;
            Mail = mail;
            LoginPasswort = loginpasswort;
            PasswordListId = passwordlistId;
        }
    }



    public class Password
    {
        public string Description { get; set; }
        public string Key { get; set; }
        public string PasswordListId { get; set; }
        public Password(string description, string key, string passwordlistId)
        {
            Description = description;
            Key = key;
            PasswordListId = passwordlistId;
        }
    }
}
