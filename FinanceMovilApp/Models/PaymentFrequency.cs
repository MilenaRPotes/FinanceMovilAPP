using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceMovilApp.Models
{
    public enum PaymentFrequency
    {
        None, //pago unico / one-time payment
        Weekly, //semanal / weekly
        BiWeekly, //quincenal / bi-weekly
        Monthly, //mensual / monthly
        yearly //anual / yearly
    }
}
