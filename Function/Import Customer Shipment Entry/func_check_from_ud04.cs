  var ud04 = Db.UD04.Where(e => 
            e.Key5 == "SuratJalan" 
            && e.Key2 == this.DNNo
            && e.ShortChar03 == this.PONum 
            && e.Number01 == this.POItem
            && e.CheckBox01 == true
          ).FirstOrDefault();
 this.output = ud04 != null ? true : false;