using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewAPP.Models
{
    public class NomenclatureItems
    {
        public int Id { get; set; }
        public string Name { get; set; } //имя
        public string InternalArticle { get; set; } //внешний номер
        public string ExternalArticle { get; set; } //номер клиента
        public string Characteristic { get; set; } //доп характеристика
        public string SerialNamber { get; set; } //серийный номер
        public string Unit { get; set; } //шт
        public string AddressCell { get; set; } //адресная ячейка
        public string OldQuantity { get; set; } //сколько было
        public string OperationTypeIn { get; set; } //добавили
        public string OperationTypeOut { get; set; } //убавили
        public string NewQuantity { get; set; } = string.Empty; //остаток
        public string UnitPrice { get; set; } //цена за ед.товара
        public string TotalPrice { get; set; } //цена за общее кол-во товара
        public DateTime? OperationDate { get; set; }

    }
}
