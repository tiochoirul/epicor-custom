
this.response = "";
string resPackNum = "";
try{

  using(var txScope = Erp.ErpContext.CreateDefaultTransactionScope()){
    CallService<Erp.Contracts.CustShipSvcContract>(svc => {
      int idx = 0;
      
      Dictionary<string, Tuple<Erp.Tablesets.ShipHeadRow, List<Erp.Tablesets.ShipDtlRow>>> orderDict = new Dictionary<string, Tuple<Erp.Tablesets.ShipHeadRow, List<Erp.Tablesets.ShipDtlRow>>>();
      
      foreach(var r in ds.UD04){
        var part = Db.Part.Where(x => x.Company == callContextClient.CurrentCompany && x.PartNum == r.ShortChar04).FirstOrDefault();
        var cust = Db.Customer.Where(x => x.Company == callContextClient.CurrentCompany && x.CustNum == 13).FirstOrDefault();
        
        if (part != null && r.ShortChar20 == "OK") {
          if (!orderDict.ContainsKey(r.Key2)) {
            Erp.Tablesets.ShipHeadRow Header = new Erp.Tablesets.ShipHeadRow();
            Header.Company = callContextClient.CurrentCompany;
            Header.ShipDate = r.Date01;
            Header.CustNum = cust.CustNum;
            Header.ShipToCustNum = cust.CustNum;
            Header.LabelComment = r.Key2;
            
            Erp.Tablesets.ShipDtlRow Detail = new Erp.Tablesets.ShipDtlRow();
            Detail.OrderNum = Convert.ToInt32(r.Number03);
            Detail.OrderLine = Convert.ToInt32(r.Number04);
            Detail.OrderRelNum = 1;
            Detail.CustNum = cust.CustNum;
            Detail.PartNum = part.PartNum;
            Detail.LineDesc = part.PartDescription;
            Detail.OurInventoryShipQty = r.Number02;
            Detail.DisplayInvQty = r.Number02;
            Detail.WarehouseCode = "FGB";
            Detail.BinNum = "1";
            Detail.LotNum = "59";
            Detail.InventoryShipUOM = part.UOMClassID;
            
            //Additional information not save DB, only for update UD01
            Detail.ShipComment = r.Key1;
            
            List<Erp.Tablesets.ShipDtlRow> ListDetail = new List<Erp.Tablesets.ShipDtlRow>();
            ListDetail.Add(Detail);
            
            orderDict.Add(r.Key2, Tuple.Create(Header, ListDetail));
          } else {
            var currentDict = orderDict[r.Key2];
            
            Erp.Tablesets.ShipDtlRow Detail = new Erp.Tablesets.ShipDtlRow();
            Detail.OrderNum = Convert.ToInt32(r.Number03);
            Detail.OrderLine = Convert.ToInt32(r.Number04);
            Detail.OrderRelNum = 1;
            Detail.CustNum = cust.CustNum;
            Detail.PartNum = part.PartNum;
            Detail.LineDesc = part.PartDescription;
            Detail.OurInventoryShipQty = r.Number02;
            Detail.DisplayInvQty = r.Number02;
            Detail.WarehouseCode = "FGB";
            Detail.BinNum = "1";
            Detail.LotNum = "59";
            Detail.InventoryShipUOM = part.IUM;
            
            //Additional information not save DB, only for update UD01
            Detail.ShipComment = r.Key1;
            
            currentDict.Item2.Add(Detail);
          }
        } 
        
        idx++;
      
      }
      
      //throw new Ice.BLException($"{orderDict.Count.ToString()}");
      
      if (orderDict != null) {
        string creditMsg = "";
        
        foreach(KeyValuePair<string, Tuple<Erp.Tablesets.ShipHeadRow, List<Erp.Tablesets.ShipDtlRow>>> kvp in orderDict) {
          Erp.Tablesets.ShipHeadRow HeaderTuple = kvp.Value.Item1;
          List<Erp.Tablesets.ShipDtlRow> ListDetailTuple = kvp.Value.Item2; 
          
          Erp.Tablesets.CustShipTableset SOT = new Erp.Tablesets.CustShipTableset();
          svc.GetNewShipHead(ref SOT);
          SOT.ShipHead[0].ShipDate = HeaderTuple.ShipDate;
          SOT.ShipHead[0].CustNum = HeaderTuple.CustNum;
          SOT.ShipHead[0].ShipToCustNum = HeaderTuple.ShipToCustNum;
          SOT.ShipHead[0].LabelComment = HeaderTuple.LabelComment;
          
          svc.Update(ref SOT);
          
          var packNum = SOT.ShipHead[0].PackNum;
          
          foreach(var item in ListDetailTuple) {
          
            svc.GetNewShipDtl(ref SOT, packNum);
            svc.GetOrderInfo(item.OrderNum, out creditMsg, ref SOT);
            svc.GetOrderLineInfo(ref SOT, SOT.ShipDtl[0].PackLine, item.OrderLine, item.PartNum);
            svc.GetOrderRelInfo(ref SOT, SOT.ShipDtl[0].PackLine, item.OrderRelNum, true);
            
            SOT.ShipDtl[0].CustNum = item.CustNum;
            //SOT.ShipDtl[0].OrderNum = item.OrderNum;
            //SOT.ShipDtl[0].OrderLine = item.OrderLine;
            //SOT.ShipDtl[0].OrderRelNum = item.OrderRelNum;
            //SOT.ShipDtl[0].PartNum = item.PartNum;
            //SOT.ShipDtl[0].LineDesc = item.LineDesc;
            SOT.ShipDtl[0].OurInventoryShipQty = item.OurInventoryShipQty;
            SOT.ShipDtl[0].DisplayInvQty = item.DisplayInvQty;
            SOT.ShipDtl[0].WarehouseCode = item.WarehouseCode;
            SOT.ShipDtl[0].BinNum = item.BinNum;
            SOT.ShipDtl[0].LotNum = item.LotNum;
            //SOT.ShipDtl[0].InventoryShipUOM = item.InventoryShipUOM;
            //SOT.ShipDtl[0].IUM = item.InventoryShipUOM;
            //SOT.ShipDtl[0].PartNumIUM = item.InventoryShipUOM;
            
            SOT.ShipDtl[0].ShipCmpl = true;            

            svc.Update(ref SOT);
            
            this.ThisLib.updateUD04(item.ShipComment);
            
          }
          
          resPackNum += System.Environment.NewLine + " => DN No: " + HeaderTuple.LabelComment + ", PackNum: " + packNum;
          
        }
        
        this.response = orderDict.Count.ToString() + " data has been upload...";
      
      } else {
        this.response = "Data not generated, please check Part Number or other data...";
      }
    });
    txScope.Complete();
    
  }
} catch(Exception e){
  this.response = $"{e.Message}";
  //throw new Ice.BLException($"{e.Message}");
}

this.response += System.Environment.NewLine + resPackNum;
