int lineReader = 0;
int idxData = 0;

var serverPath = new Ice.Hosting.ServerDataPath(this.Session);
var rootFolderPath = System.IO.Path.Combine(serverPath.UserDataDirectory, "Uploads", this.path);

try{
  using (StreamReader reader = new StreamReader(rootFolderPath))
  {
    string delimiter = "";
    Ice.Tablesets.UD04Tableset DataSet = new Ice.Tablesets.UD04Tableset();
    while (!reader.EndOfStream){
      lineReader++;
      string readData = reader.ReadLine();
      if(lineReader >= 1 && lineReader <= 5){
        delimiter = "/t";// readData.Substring(4, 1);
      } else {
        string[] dataSplit = Regex.Split(readData, @"\t"); //readData.Split(delimiter);
        if(dataSplit[0] != ""){
          
            CallService<Ice.Contracts.UD04SvcContract>(svc => {
                
                DateTime dnDate;
                DateTime.TryParseExact(dataSplit[1], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dnDate);
                
                string dnNo = dataSplit[0];
                string dnStatus = dataSplit[2];
                string plantDesc = dataSplit[13];
                string poNum = dataSplit[14];
                string poItem = dataSplit[30];
                string partNum = dataSplit[31];
                string partDesc = dataSplit[32];
                decimal qty = Convert.ToDecimal(dataSplit[37]);
                
                bool isExists = this.ThisLib.checkFromDb(dnNo, poNum, Convert.ToInt32(poItem));
                //var part = Db.Part.Where(e => e.PartNum == partNum).FirstOrDefault();
                
                string order = "0/0";
                order = ThisLib.getSONum(poNum, poItem.ToString());
                string ordNum = "0";
                string lineNum = "0";
                if (order != "0/0") {
                  string[] arrOrd = Regex.Split(order, "/");
                
                  if (arrOrd.Count() != 0) {
                    ordNum = arrOrd[0];
                    lineNum = arrOrd[1];
                  }               
                }
                
                int inOrdNum = Convert.ToInt32(ordNum);
                int inLineNum = Convert.ToInt32(lineNum);
                //var ordRel = Db.OrderHed.Where(x => x.Company == this.Session.CompanyID && x.OrderNum == inOrdNum).FirstOrDefault().OrderStatus;
                
                if(!isExists && order != "0/0"){
                  svc.GetaNewUD04(ref DataSet);
                  
                  //DataSet.UD01[idxData]["No"] = idxData + 1;
                  DataSet.UD04[idxData].Company = callContextClient.CurrentCompany;
                  DataSet.UD04[idxData].Key1 = Guid.NewGuid().ToString().ToUpper(); 
                  DataSet.UD04[idxData].Key2 = dnNo;
                  DataSet.UD04[idxData].Key5 = "SuratJalan";
                  
                  DataSet.UD04[idxData].Character01 = plantDesc;
                  DataSet.UD04[idxData].Character02 = partDesc;
                  
                  DataSet.UD04[idxData].ShortChar01 = dnNo;
                  DataSet.UD04[idxData].ShortChar02 = dnStatus;
                  DataSet.UD04[idxData].ShortChar03 = poNum;
                  DataSet.UD04[idxData].ShortChar04 = partNum;
                  
                  DataSet.UD04[idxData].Number01 = Convert.ToInt32(poItem);
                  DataSet.UD04[idxData].Number02 = qty;
                  DataSet.UD04[idxData].Number03 = Convert.ToInt32(ordNum);
                  DataSet.UD04[idxData].Number04 = Convert.ToInt32(lineNum);
                  
                  DataSet.UD04[idxData].Date01 = dnDate;
                  DataSet.UD04[idxData].Character03 = DateTime.Now.ToString("yyyyMMdd HHmmss");
                  
                  DataSet.UD04[idxData].ShortChar20 = "OK";
                  
                  //svc.Update(ref DataSet);
                  idxData++;
                } else {
                  string ketExists = "";
                  if (isExists) {
                    ketExists = ": Data has been create in Cust Shipment.";
                  }
                  svc.GetaNewUD04(ref DataSet);
                  
                  //DataSet.UD01[idxData]["No"] = idxData + 1;
                  DataSet.UD04[idxData].Company = callContextClient.CurrentCompany;
                  DataSet.UD04[idxData].Key1 = Guid.NewGuid().ToString().ToUpper(); 
                  DataSet.UD04[idxData].Key2 = dnNo;
                  DataSet.UD04[idxData].Key5 = "SuratJalan";
                  
                  DataSet.UD04[idxData].Character01 = plantDesc;
                  DataSet.UD04[idxData].Character02 = partDesc;
                  
                  DataSet.UD04[idxData].ShortChar01 = dnNo;
                  DataSet.UD04[idxData].ShortChar02 = dnStatus;
                  DataSet.UD04[idxData].ShortChar03 = poNum;
                  DataSet.UD04[idxData].ShortChar04 = partNum;
                  
                  DataSet.UD04[idxData].Number01 = Convert.ToInt32(poItem);
                  DataSet.UD04[idxData].Number02 = qty;
                  DataSet.UD04[idxData].Number03 = Convert.ToInt32(ordNum);
                  DataSet.UD04[idxData].Number04 = Convert.ToInt32(lineNum);
                  
                  DataSet.UD04[idxData].Date01 = dnDate;
                  DataSet.UD04[idxData].Character03 = DateTime.Now.ToString("yyyyMMdd HHmmss");
                  
                  DataSet.UD04[idxData].ShortChar20 = "Failed" + ketExists;
                  
                  //svc.Update(ref DataSet);
                  idxData++;
                }
              });
        }
      }
    }
    
    if (DataSet != null) {
      this.result = DataSet;
      using(var txScope = Erp.ErpContext.CreateDefaultTransactionScope()){
        CallService<Ice.Contracts.UD04SvcContract>(svc => {
         svc.Update(ref DataSet);
        });
        txScope.Complete();
      }
    }
  }
} catch(Exception ex){
  throw new Ice.BLException($"{lineReader} -> {ex.Message}");
}
