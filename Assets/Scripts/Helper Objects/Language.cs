using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Language
{

    private static string[] consonants = new string[] { "p", "t", "k", "m", "n", "ng", "f", "th", "s", "kh", "h", "l", "r" };
    private static string[] vowels = new string[] { "i", "e", "a", "o", "u" };

    public Language()
    {
        // placeholder
    }

    public string GenerateName()
    {
        int syllables = UnityEngine.Random.Range(1, 4);
        StringBuilder name = new StringBuilder();

        for (int i = 0; i < syllables; i++)
        {   
            name.Append(GetConsonant());
            name.Append(GetVowel());
            if (Random.Range(0, 1) == 1)
            {
                name.Append(GetConsonant());
            }
        }

        name[0] = char.ToUpper(name[0]);
        return name.ToString();
    }

    private string GetConsonant()
    {
        int consonantIndex = Random.Range(0, consonants.Length);
        return consonants[consonantIndex];
    }

    private string GetVowel()
    {
        int vowelIndex = Random.Range(0, vowels.Length);
        return vowels[vowelIndex];
    }
}
