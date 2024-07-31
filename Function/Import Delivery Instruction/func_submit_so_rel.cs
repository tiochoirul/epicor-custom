
this.output = "";
string relSuccess = "";
try{

  using(var txScope = Erp.ErpContext.CreateDefaultTransactionScope()){
    CallService<Erp.Contracts.SalesOrderSvcContract>(svc => {
      Dictionary<string, Tuple<Erp.Tablesets.OrderDtlRow, List<Erp.Tablesets.OrderRelRow>>> orderDict = new Dictionary<string, Tuple<Erp.Tablesets.OrderDtlRow, List<Erp.Tablesets.OrderRelRow>>>();
      
      foreach(var r in ds.UD03){
        int ordNum = Convert.ToInt32(r.Number03);
        int ordLine = Convert.ToInt32(r.Number04);
        string key = ordNum.ToString() + ordLine.ToString();
        
        if (r.ShortChar20 == "OK") {
          if (!orderDict.ContainsKey(key)) {
            Erp.Tablesets.OrderDtlRow Detail = new Erp.Tablesets.OrderDtlRow();
            Detail.OrderNum = ordNum;
            Detail.OrderLine = ordLine;
            
            Erp.Tablesets.OrderRelRow Release = new Erp.Tablesets.OrderRelRow();
            Release.OrderRelNum = 0;
            Release.OrderNum = ordNum;
            Release.OrderLine = ordLine;
            Release.SellingReqQty = r.Number02;
            Release.OurReqQty = r.Number02;
            Release.Reference = r.Key2;
            
            List<Erp.Tablesets.OrderRelRow> ListRelease = new List<Erp.Tablesets.OrderRelRow>();
            ListRelease.Add(Release);
            
            orderDict.Add(key, Tuple.Create(Detail, ListRelease));
          } else {
            var currentDict = orderDict[key];
            
            Erp.Tablesets.OrderRelRow Release = new Erp.Tablesets.OrderRelRow();
            Release.OrderRelNum = currentDict.Item2.Last().OrderRelNum + 1;
            Release.OrderNum = ordNum;
            Release.OrderLine = ordLine;
            Release.SellingReqQty = r.Number02;
            Release.OurReqQty = r.Number02;
            Release.Reference = r.Key2;
            
            currentDict.Item2.Add(Release);
          }
        }
      }
      
      int idx = 0;
      if (orderDict != null) {
        foreach(KeyValuePair<string, Tuple<Erp.Tablesets.OrderDtlRow, List<Erp.Tablesets.OrderRelRow>>> kvp in orderDict) {
          Erp.Tablesets.OrderDtlRow HeaderTuple = kvp.Value.Item1;
          List<Erp.Tablesets.OrderRelRow> ListDetailTuple = kvp.Value.Item2; 
          
          foreach(var item in ListDetailTuple) {
            Erp.Tablesets.SalesOrderTableset SOT = new Erp.Tablesets.SalesOrderTableset();
            
            if (item.OrderRelNum == 0) {
              string whereClauseOrderHed = "OrderNum = '" + HeaderTuple.OrderNum.ToString() + "' ";
              string whereClauseOrderDtl = "OrderLine = '" + HeaderTuple.OrderLine.ToString() + "' ";
              string whereClauseOrderRel = "OrderRelNum = '0' ";
              bool morePage = false;
              SOT = svc.GetRows(whereClauseOrderHed, "", "", whereClauseOrderDtl, "", "", "", whereClauseOrderRel, "", "", "", "", "", "", "", "", "", "", 100, 1, out morePage);
              
              SOT.OrderRel[item.OrderRelNum].SellingReqQty = item.SellingReqQty;
              SOT.OrderRel[item.OrderRelNum].OurReqQty = item.OurReqQty;
              SOT.OrderRel[item.OrderRelNum].Reference = item.Reference;
              
            } else {
              svc.GetNewOrderRel(ref SOT, item.OrderNum, item.OrderLine);
              SOT.OrderRel[item.OrderRelNum].SellingReqQty = item.SellingReqQty;
              SOT.OrderRel[item.OrderRelNum].OurReqQty = item.OurReqQty;
              SOT.OrderRel[item.OrderRelNum].Reference = item.Reference;
            }
            
            svc.Update(ref SOT);
            
            string relNum = SOT.OrderRel[0].OrderRelNum.ToString();
          
            relSuccess += " => " + item.Reference + " : " + HeaderTuple.OrderNum.ToString() + "/" + HeaderTuple.OrderLine.ToString() + "/" + relNum + System.Environment.NewLine;
            
            idx++;
          }
          
        }
      }
      
      this.output += idx.ToString() + " data has been upload...";
      
    });
    txScope.Complete();
    
    //throw new Ice.BLException($"{this.dsError.Tables[0].Rows[0][0].ToString()}");
  }
} catch(Exception e){
  this.output = $"{e.Message}";
  //throw new Ice.BLException($"{e.Message}");
}

this.output += System.Environment.NewLine + relSuccess;