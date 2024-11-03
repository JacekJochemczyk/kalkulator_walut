using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http; // Klient HTTP do komunikacji z API
using Newtonsoft.Json; // Biblioteka do konwersji JSON na obiekty C#

namespace kalkulator_walut
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            comboBox1.Items.Add("EUR");
            comboBox1.Items.Add("CHF");
            comboBox1.Items.Add("GBP");
        }

        private async Task<Dictionary<string, decimal>> GetExchangeRates()
        {
            
            string url = $"https://open.er-api.com/v6/latest/USD";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ExchangeRateResponse>(responseBody);
                return data.Rates;
            }
        }

        public class ExchangeRateResponse
        {
            public Dictionary<string, decimal> Rates { get; set; }
        }

        private decimal ConvertAmount(decimal kwotaPLN, decimal rateUSD, decimal rateTarget)
        {
            // Krok 1: Przelicz kwotę z PLN na USD
            decimal kwotaUSD = kwotaPLN / rateUSD;

            // Krok 2: Przelicz kwotę z USD na walutę docelową
            decimal wynik = kwotaUSD * rateTarget;

            return wynik; // Zwraca wynik przeliczenia
        }

        private async void btnConvert_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Proszę wybrać walutę docelową.");
                    return;
                }

                var rates = await GetExchangeRates(); // Pobierz kursy

                if (rates == null || rates.Count == 0)
                {
                    MessageBox.Show("Nie udało się pobrać kursów wymiany.");
                    return;
                }

                // Pobierz kwotę w PLN od użytkownika
                if (!decimal.TryParse(textBox1.Text, out decimal kwotaPLN))
                {
                    MessageBox.Show("Proszę wpisać poprawną kwotę w PLN.");
                    return;
                }

                string targetCurrency = comboBox1.SelectedItem.ToString(); // Pobierz walutę docelową

                
                if (rates.TryGetValue("PLN", out decimal ratePLN) && rates.TryGetValue(targetCurrency, out decimal rateTarget))
                {
                    // Krok 1: Przelicz kwotę z PLN na USD
                    decimal kwotaUSD = kwotaPLN / ratePLN; // Teraz przeliczamy z PLN na USD

                    // Krok 2: Przelicz kwotę z USD na walutę docelową
                    decimal wynik = kwotaUSD * rateTarget; // Przelicz z USD na walutę docelową

                    // Wyświetl wynik w polu tekstowym
                    textBox2.Text = wynik.ToString("0.00");
                }
                else
                {
                    MessageBox.Show("Nie udało się znaleźć kursów wymiany.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd: " + ex.Message);
            }
        }
    }
}
