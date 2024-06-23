using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    public static class Validator
    {
        public static bool IsValidCountry(string country)
        {
            List<string> validCountries = new List<string> {
                "ae", "ar", "at", "au", "be", "bg", "br", "ca", "ch", "cn", "co", "cu",
                "cz", "de", "eg", "fr", "gb", "gr", "hk", "hu", "id", "ie", "il", "in",
                "it", "jp", "kr", "lt", "lv", "ma", "mx", "my", "ng", "nl", "no", "nz",
                "ph", "pl", "pt", "ro", "rs", "ru", "sa", "se", "sg", "si", "sk", "th",
                "tr", "tw", "ua", "us", "ve", "za"
            };

            return validCountries.Contains(country.ToLower());
        }

        public static bool IsValidCategory(string category)
        {
            List<string> validCategories = new List<string> {
                "business", "entertainment", "general", "health", "science", "sports", "technology"
            };

            return validCategories.Contains(category.ToLower());
        }
    }
}
