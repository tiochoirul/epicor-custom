
this.output = "";
string relSuccess = "";
try{

  using(var txScope = Erp.ErpContext.CreateDefaultTransactionScope()){
    CallService<Erp.Contracts.SalesOrderSvcContract>(svc => {
      int idx = 0;
      
      foreach(var r in ds.UD03){
        int ordNum = Convert.ToInt32(r.Number03);
        int ordLine = Convert.ToInt32(r.Number04);
        
        if (r.ShortChar20 == "OK") {
          
          Erp.Tablesets.SalesOrderTableset SOT = new Erp.Tablesets.SalesOrderTableset();
          
          /**
          string whereClauseOrderHed = "OrderNum = '" + ordNum + "' ";
          string whereClauseOrderDtl = "OrderLine = '" + ordLine + "' ";
          string whereClauseOrderRel = "OrderRelNum = '1' ";
          bool morePage = false;
          SOT = svc.GetRows(whereClauseOrderHed, "", "", whereClauseOrderDtl, "", "", "", whereClauseOrderRel, "", "", "", "", "", "", "", "", "", "", 100, 1, out morePage);
          
          if (SOT != null) {
            
          }
          
          **/
          
          svc.GetNewOrderRel(ref SOT, ordNum, ordLine);
          SOT.OrderRel[0].SellingReqQty = r.Number02;
          SOT.OrderRel[0].OurReqQty = r.Number02;
          SOT.OrderRel[0].Reference = r.Key2;
          //SOT.OrderRel[0].DemandReference = r.Key1;
          
          svc.Update(ref SOT);
          
          string relNum = SOT.OrderRel[0].OrderRelNum.ToString();
          
          relSuccess += " => SO: " + ordNum.ToString() + "/" + ordLine.ToString() + "/" + relNum + System.Environment.NewLine;
          //this.ThisLib.updateUD01(HeaderTuple.PONum, HeaderTuple.OrderDate, item.Reference, item.POLine, item.PartNum);
          
          idx++;
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