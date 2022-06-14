using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_organizer.Models
{
    public class Communal
    {
        public string Month { get; set; }
        public string Year { get; set; }

        public string Period => Year + " " + Month;

        //Властивості водопостачання
        public double WaterSupplyTarif { get; set; }
        public int PrevWaterSupply { get; set; }
        public int CurentWaterSupply { get; set; }
        public double SumWaterSupply { get; set; }
        public int WaterSupplyDifferent => CurentWaterSupply - PrevWaterSupply;

        //Властивості водовідведення
        public double DrainageTarif { get; set; }
        public int PrevDrainage { get; set; }
        public int CurentDrainage { get; set; }
        public double SumDrainage { get; set; }
        public int DrainageDifferent => CurentDrainage - PrevDrainage;

        //Властивості природний газ
        public double GasTarif { get; set; }
        public int PrevGas { get; set; }
        public int CurentGas { get; set; }
        public double SumGas { get; set; }
        public int GasDifferent => CurentGas - PrevGas;

        //Властивості доставка газу
        public double DeliveryGasTarif { get; set; }
        public double SumDeliveryGas { get; set; }

        //Властивості електроенергія
        public double ElectricTarif { get; set; }
        public int PrevElectric { get; set; }
        public int CurentElectric { get; set; }
        public double SumElectric { get; set; }
        public int ElectricDifferent => CurentElectric - PrevElectric;

        //Властивості квартплата
        public double RentTarif { get; set; }
        public double SumRent { get; set; }

        //Властивості охорона
        public double ProtectionTarif { get; set; }
        public double SumProtection { get; set; }

        //Властивості вивіз сміття
        public double GarbageTarif { get; set; }
        public double SumGarbage { get; set; }

        //Властивості домофон
        public double IntercomTarif { get; set; }
        public double SumIntercom { get; set; }

        //Властивості інтернет
        public double InternetTarif { get; set; }
        public double SumInternet { get; set; }

        //Загальна сума
        public double TotalSum => SumWaterSupply + SumDrainage + SumGas + SumDeliveryGas + SumElectric + SumRent + SumProtection + SumGarbage + SumIntercom + SumInternet;
    }
}
