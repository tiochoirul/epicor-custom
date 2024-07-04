  int month = this.date.Value.Month;
  int year = this.date.Value.Year;
  var ud01 = Db.UD01.Where(e => 
            e.Key5 == "SalesOrder" 
            && e.Key2 == this.poNum 
            && e.ShortChar04 == this.poItem
            && e.Character02 == this.partNum 
            && e.Character01 == this.plantAHM
            && e.Date01.Value.Month == month
            && e.Date01.Value.Year == year
            && e.CheckBox01 == true
          ).FirstOrDefault();
 this.duplicate = ud01 != null ? true : false;