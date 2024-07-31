this.soNumOutput = "";
this.responseGenerate = "";
try{

  using(var txScope = Erp.ErpContext.CreateDefaultTransactionScope()){
    CallService<Erp.Contracts.SalesOrderSvcContract>(svc => {
      int idx = 0;
      
      Dictionary<string, Tuple<Erp.Tablesets.OrderHedRow, List<Erp.Tablesets.OrderDtlRow>>> orderDict = new Dictionary<string, Tuple<Erp.Tablesets.OrderHedRow, List<Erp.Tablesets.OrderDtlRow>>>();
      
      foreach(var r in ds.UD01){
        var part = Db.Part.Where(x => x.Company == callContextClient.CurrentCompany && x.PartNum == r.Character02).FirstOrDefault();
        var cust = Db.Customer.Where(x => x.Company == callContextClient.CurrentCompany && x.CustNum == 13).FirstOrDefault();
        
        if (part != null && r.ShortChar20 == "OK") {
          if (!orderDict.ContainsKey(r.Key2)) {
            Erp.Tablesets.OrderHedRow Header = new Erp.Tablesets.OrderHedRow();
            Header.Company = callContextClient.CurrentCompany;
            Header.OrderDate = r.Date01;
            Header.NeedByDate = r.Date02;
            Header.CustNum = cust.CustNum;
            Header.ShipToCustNum = cust.CustNum;
            Header.PONum = r.Key2;        
            Header.TermsCode = r.Number01.ToString() + "H";
            Header.CurrencyCode = r.ShortChar01;
            
            Erp.Tablesets.OrderDtlRow Detail = new Erp.Tablesets.OrderDtlRow();
            Detail.OrderLine = 0;
            Detail.PartNum = part.PartNum;
            Detail.LineDesc = part.PartDescription;
            Detail.SellingQuantity = r.Number03;
            Detail.DocDspUnitPrice = r.Number04;
            
            //Additional information not save DB, only for update UD01
            Detail.POLine = r.ShortChar04;
            Detail.Reference = r.Character01;
            
            List<Erp.Tablesets.OrderDtlRow> ListDetail = new List<Erp.Tablesets.OrderDtlRow>();
            ListDetail.Add(Detail);
            
            orderDict.Add(r.Key2, Tuple.Create(Header, ListDetail));
          } else {
            var currentDict = orderDict[r.Key2];
            
            Erp.Tablesets.OrderDtlRow Detail = new Erp.Tablesets.OrderDtlRow();
            Detail.OrderLine = currentDict.Item2.Last().OrderLine + 1;
            Detail.PartNum = part.PartNum;
            Detail.LineDesc = part.PartDescription;
            Detail.SellingQuantity = r.Number03;
            Detail.DocDspUnitPrice = r.Number04;
            
            //Additional information not save DB, only for update UD01
            Detail.POLine = r.ShortChar04;
            Detail.Reference = r.Character01;
            
            currentDict.Item2.Add(Detail);
          }
        } else {
          this.resultError += r.Key2 + "/" + r.Character02 + "~";
        }
        
        idx++;
      
      }
      
      //throw new Ice.BLException($"{orderDict.Count.ToString()}");
      
      if (orderDict != null) {
        foreach(KeyValuePair<string, Tuple<Erp.Tablesets.OrderHedRow, List<Erp.Tablesets.OrderDtlRow>>> kvp in orderDict) {
          Erp.Tablesets.OrderHedRow HeaderTuple = kvp.Value.Item1;
          List<Erp.Tablesets.OrderDtlRow> ListDetailTuple = kvp.Value.Item2; 
          
          Erp.Tablesets.SalesOrderTableset SOT = new Erp.Tablesets.SalesOrderTableset();
          svc.GetNewOrderHed(ref SOT);
          SOT.OrderHed[0].Company = HeaderTuple.Company;
          SOT.OrderHed[0].OrderDate = HeaderTuple.OrderDate;
          SOT.OrderHed[0].NeedByDate = HeaderTuple.NeedByDate;
          SOT.OrderHed[0].CustNum = HeaderTuple.CustNum;
          SOT.OrderHed[0].BTCustNum = HeaderTuple.CustNum;
          SOT.OrderHed[0].ShipToCustNum = HeaderTuple.CustNum;
          SOT.OrderHed[0].PONum = HeaderTuple.PONum;        
          SOT.OrderHed[0].TermsCode = HeaderTuple.TermsCode;
          SOT.OrderHed[0].CurrencyCode = HeaderTuple.CurrencyCode;
          
          svc.Update(ref SOT);
          
          var ordNum = SOT.OrderHed[0].OrderNum;
          
          int line = 0;
          foreach(var item in ListDetailTuple) {
            svc.GetNewOrderDtl(ref SOT, ordNum);
            SOT.OrderDtl[line].PartNum = item.PartNum;
            SOT.OrderDtl[line].LineDesc = item.LineDesc;
            SOT.OrderDtl[line].SellingQuantity = item.SellingQuantity;
            SOT.OrderDtl[line].DocDspUnitPrice = item.DocDspUnitPrice;
            SOT.OrderDtl[line].OrderQty = item.SellingQuantity;
            SOT.OrderDtl[line].DocUnitPrice = item.DocDspUnitPrice;
            SOT.OrderDtl[line].POLine = item.POLine;
            
            svc.Update(ref SOT);
            
            //this.ThisLib.updateUD01(HeaderTuple.PONum, HeaderTuple.OrderDate, item.Reference, item.POLine, item.PartNum);
            line++;
          }
          
          this.soNumOutput += HeaderTuple.PONum + "/" + ordNum + "~";
          
        } 
        this.responseGenerate = orderDict.Count.ToString() + " data has been upload...";
      
      } else {
        this.responseGenerate = "Data not generated, please check Part Number or other data...";
      }
      //svc.Update(ref ds);
    });
    txScope.Complete();
    
    this.ThisLib.deleteUD01(this.Session.CompanyID);
    //throw new Ice.BLException($"{this.dsError.Tables[0].Rows[0][0].ToString()}");
  }
} catch(Exception e){
  this.soNumOutput = "Throw Error, Rollback Transaction";
  this.responseGenerate = $"Exception: {e.Message}";
  //throw new Ice.BLException($"{e.Message}");
}
