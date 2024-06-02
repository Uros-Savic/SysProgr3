using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public static class Validator
    {
        public static async Task<bool> IsValidCountry(string country)
        {
            List<string> validCountries = new List<string> {
            "ae", "ar", "at", "au", "be", "bg", "br", "ca", "ch", "cn", "co", "cu",
            "cz", "de", "eg", "fr", "gb", "gr", "hk", "hu", "id", "ie", "il", "in",
            "it", "jp", "kr", "lt", "lv", "ma", "mx", "my", "ng", "nl", "no", "nz",
            "ph", "pl", "pt", "ro", "rs", "ru", "sa", "se", "sg", "si", "sk", "th",
            "tr", "tw", "ua", "us", "ve", "za"
        };

            return await Task.FromResult(validCountries.Contains(country.ToLower()));
        }

        public static async Task<bool> IsValidCategory(string category)
        {
            List<string> validCategories = new List<string> {
            "business", "entertainment", "general", "health", "science", "sports", "technology"
        };

            return await Task.FromResult(validCategories.Contains(category.ToLower()));
        }
    }
}
