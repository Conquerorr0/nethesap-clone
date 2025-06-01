using System;
using System.Collections.Generic;
using Nethesap.UI.ViewModels;

namespace Nethesap.UI.Localization
{
    public static class Translator
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "Türkçe", new Dictionary<string, string>
                {
                    { "Dashboard", "Gösterge Paneli" },
                    { "Customers", "Müşteriler" },
                    { "Products", "Ürünler" },
                    { "Sales", "Satışlar" },
                    { "Accounts", "Hesaplar" },
                    { "Settings", "Ayarlar" },
                    { "Customer", "Müşteri" },
                    { "Save", "Kaydet" },
                    { "Cancel", "İptal" },
                    { "Add", "Ekle" },
                    { "Edit", "Düzenle" },
                    { "Delete", "Sil" },
                    { "Search", "Ara" },
                    { "DarkTheme", "Koyu Tema" },
                    { "LightTheme", "Açık Tema" }
                }
            },
            {
                "English", new Dictionary<string, string>
                {
                    { "Dashboard", "Dashboard" },
                    { "Customers", "Customers" },
                    { "Products", "Products" },
                    { "Sales", "Sales" },
                    { "Accounts", "Accounts" },
                    { "Settings", "Settings" },
                    { "Customer", "Customer" },
                    { "Save", "Save" },
                    { "Cancel", "Cancel" },
                    { "Add", "Add" },
                    { "Edit", "Edit" },
                    { "Delete", "Delete" },
                    { "Search", "Search" },
                    { "DarkTheme", "Dark Theme" },
                    { "LightTheme", "Light Theme" }
                }
            }
        };

        private static string _currentLanguage = "Türkçe";

        // Dil değişikliği olayı
        public static event Action<string> LanguageChanged;

        public static string CurrentLanguage
        {
            get => _currentLanguage;
            set 
            {
                if (_currentLanguage != value)
                {
                    Console.WriteLine($"Translator: Language changing from {_currentLanguage} to {value}");
                    _currentLanguage = value;
                    
                    // Dil değişikliği olayını tetikle
                    LanguageChanged?.Invoke(value);
                }
            }
        }

        public static string Translate(string key, string language = null)
        {
            // Dil belirtilmemişse, mevcut dili kullan
            language = language ?? CurrentLanguage;
            
            // Dil veya anahtar bulunamazsa, anahtarın kendisini döndür
            if (!Translations.ContainsKey(language) || !Translations[language].ContainsKey(key))
            {
                return key;
            }
            
            return Translations[language][key];
        }
        
        // Mevcut dil için tüm çevirileri al
        public static Dictionary<string, string> GetAllTranslations()
        {
            if (Translations.TryGetValue(CurrentLanguage, out var translations))
            {
                return translations;
            }
            
            return new Dictionary<string, string>();
        }
        
        // Desteklenen dilleri al
        public static string[] GetSupportedLanguages()
        {
            var languages = new string[Translations.Count];
            int index = 0;
            
            foreach (var key in Translations.Keys)
            {
                languages[index++] = key;
            }
            
            return languages;
        }
    }
} 