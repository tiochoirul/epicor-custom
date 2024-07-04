string modulID = "RcvDtl";
decimal zeroQty = 0M;
this.result = "";
bool qtyIMvsReceiptStatus = false; 

if (dsUD05.Tables[0].Rows.Count > 0) {
  try
  {
    decimal totalIM = 0;
    string packSlip ="";
    int packLine = 0;
    int venNum = 0;
    
    foreach(DataRow r in dsUD05.Tables[0].Rows) 
    {
      totalIM += Convert.ToDecimal(r["Calculated_QtyIM"]);
      packSlip = r["UD05_Key2"].ToString();
      packLine = Convert.ToInt32(r["UD05_Key3"]);
      venNum = Convert.ToInt32(r["UD05_Key1"]);
    }   
    
    var dbRcvDtlRecs = Db.RcvDtl.With(LockHint.NoLock).Where(x => x.Company == this.Session.CompanyID && x.VendorNum == venNum && x.PackSlip == packSlip && x.PackLine == packLine).FirstOrDefault();
    decimal qtyRcv = dbRcvDtlRecs.OurQty;
   
    // Cek Total IM & Receipt
    if (qtyRcv != totalIM) {
      this.result = "FAILED";
      throw new Ice.BLException($"Total Issue tidak sama dengan Qty Receipt.\nQty Receipt: " + qtyRcv + "\nTotal Issue: " + totalIM);
    }
    
    //Start of Proses update QtyIM
    foreach(DataRow r in dsUD05.Tables[0].Rows) 
    {
      decimal QtyIM = Convert.ToDecimal(r["Calculated_QtyIM"]);
      
      if (QtyIM > 0) 
      {
        Ice.Tablesets.UD05Tableset DataSet = new Ice.Tablesets.UD05Tableset();   
        using(var txScope = Erp.ErpContext.CreateDefaultTransactionScope())
        {
          CallService<Ice.Contracts.UD05SvcContract>(svc => 
          {
            string whereClauseUD05 = "Key1 = '" + r["UD05_Key1"] + "' AND Key2 = '"+ r["UD05_Key2"] +"' AND Key3 = '"+ r["UD05_Key3"] +"' AND Key5 = '"+ modulID +"' AND CheckBox01 = true AND Character02 = '"+ r["UD05_Character02"] + "' " ;
            bool morePage = true;
            DataSet = svc.GetRows(whereClauseUD05, "", 100, 1, out morePage);
            
            if (DataSet.UD05.Count != 0) 
            {
        
              foreach(var x in DataSet.UD05) 
              {
                x.Number07 = QtyIM;
               
                x.CheckBox01 = true; // Tanda row ini masih aktif
               
                x.RowMod = "U";
              }
            }
            svc.Update(ref DataSet);
            this.result = "OK";
          });
          txScope.Complete();
        }    
      }
    }
    //End of Proses update QtyIM
    
  } catch(Exception ex){
    this.result = "FAILED";
    throw new Ice.BLException($"getData: -> {ex.Message}");
  }
}

