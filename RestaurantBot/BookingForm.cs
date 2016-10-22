using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RestaurantBot
{
    [Serializable]
    public class BookingForm
    {
        [Prompt("What day and time would you like to make a booking for? Remember to specify am or pm.")]
        public DateTime Date { get; set; }
        // Don't forget to make datetime nullable if it's optional!
        [Prompt("What is your preferred time?")]
        [Optional]
        //[Template(TemplateUsage.StatusFormat, "{&}: {:t}", FieldCase = CaseNormalization.None)]
        public DateTime? Time { get; set; }
        [Prompt("Your name?")]
        public string Name { get; set; }
        [Prompt("How many people will there be?")]
        public int NumPeople { get; set; }
        [Prompt("Can I get your phone number please?")]
        public string PhNum { get; set; }
        [Prompt("Enter any additional requests or notes. Say None if you don't have any.")]
        [Optional]
        public string Requests { get; set; }
        public static IForm<BookingForm> BuildForm()
        {
            return new FormBuilder<BookingForm>()
                .Field(nameof(Date))
                .Field(nameof(Time), active: IsTimeAdded)
                .AddRemainingFields()
                .Field(nameof(PhNum), validate: ValidatePhNum)
                .Confirm("Confirm booking for {NumPeople} people on {Date}? (Y/N)")
                .Build();
        }
        private static bool IsTimeAdded(BookingForm state)
        {
            if (state.Date.TimeOfDay.TotalSeconds == 0)
            {
                return true;
            }
            return false;
        }
        private static Task<ValidateResult> ValidatePhNum(BookingForm state, object response)
        {
            var result = new ValidateResult();
            string phoneNumber = string.Empty;
            if (IsPhNum((string)response))
            {
                result.IsValid = true;
                result.Value = response;
            }
            else
            {
                result.IsValid = false;
                result.Feedback = "Make sure the phone number you entered are all numbers.";
            }
            return Task.FromResult(result);
        }
        private static bool IsPhNum(string response)
        {
            if (Regex.IsMatch(response, @"^\d+$"))
            {
                return true;
            }
            return false;
        }
    }
}