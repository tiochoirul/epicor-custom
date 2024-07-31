int lineReader = 0;
int idxData = 0;

var serverPath = new Ice.Hosting.ServerDataPath(this.Session);
var rootFolderPath = System.IO.Path.Combine(serverPath.UserDataDirectory, "Uploads", this.path);
bool locked = true;

try{
  //throw new Ice.BLException($"{rootFolderPath} :: {resFS}");
  
  using (StreamReader reader = new StreamReader(rootFolderPath))
  {
    string delimiter = "";
    Ice.Tablesets.UD03Tableset DataSet = new Ice.Tablesets.UD03Tableset();
    while (!reader.EndOfStream){
      lineReader++;
      string readData = reader.ReadLine();
      if(lineReader == 1){
        delimiter = "/t";// readData.Substring(4, 1);
      } else {
        
        string[] dataSplit = Regex.Split(readData, @"\t"); //readData.Split(delimiter);
        if(dataSplit[0] != ""){
        
            CallService<Ice.Contracts.UD03SvcContract>(svc => {
                
                DateTime diRcvDate;
                DateTime.TryParseExact(dataSplit[16], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out diRcvDate);
                
                string diNo = dataSplit[0];
                string gate = dataSplit[1];
                string poNum = dataSplit[2];
                int poItem = Convert.ToInt32(dataSplit[3]);
                string partNum = dataSplit[6];
                decimal qty = Convert.ToDecimal(dataSplit[8]);
                
                string uom = dataSplit[9];
                string dnNo = dataSplit[22];
                string plantID = dataSplit[23];
                string plantName = dataSplit[24];
                
                bool isExists = this.ThisLib.checkFromDb(diNo, gate, poNum, poItem, partNum);
                var part = Db.Part.Where(e => e.PartNum == partNum).FirstOrDefault();

                //string order = ThisLib.getSONum(poNum, poItem.ToString(), partNum);
                string order = "0/0";
                order = ThisLib.getSONum(poNum, poItem.ToString(), partNum);
                
                string ordNum = "0";
                string lineNum = "0";
                if (order != "0/0") {
                  string[] arrOrd = Regex.Split(order, "/");
                
                  if (arrOrd.Count() != 0) {
                    ordNum = arrOrd[0];
                    lineNum = arrOrd[1];
                  }               
                }
                
                if(!isExists && part != null && ordNum != "0" && lineNum != "0"){
                  svc.GetaNewUD03(ref DataSet);
                  
                  //DataSet.UD01[idxData]["No"] = idxData + 1;
                  DataSet.UD03[idxData].Company = callContextClient.CurrentCompany;
                  DataSet.UD03[idxData].Key1 = Guid.NewGuid().ToString().ToUpper(); 
                  DataSet.UD03[idxData].Key2 = diNo;
                  DataSet.UD03[idxData].Key5 = "DeliveryInstruction";
                  
                  DataSet.UD03[idxData].Character01 = plantName;
                  
                  DataSet.UD03[idxData].ShortChar01 = gate;
                  DataSet.UD03[idxData].ShortChar02 = poNum;
                  DataSet.UD03[idxData].ShortChar03 = plantID;
                  DataSet.UD03[idxData].ShortChar04 = partNum;
                  DataSet.UD03[idxData].ShortChar05 = uom;
                  DataSet.UD03[idxData].ShortChar06 = dnNo;
                  
                  DataSet.UD03[idxData].Number01 = poItem;
                  DataSet.UD03[idxData].Number02 = qty;
                  DataSet.UD03[idxData].Number03 = Convert.ToInt32(ordNum);
                  DataSet.UD03[idxData].Number04 = Convert.ToInt32(lineNum);
                  
                  DataSet.UD03[idxData].Date01 = diRcvDate;
                  DataSet.UD03[idxData].Character02 = DateTime.Now.ToString("yyyyMMdd HHmmss");
                  DataSet.UD03[idxData].ShortChar20 = "OK";
                  
                  //svc.Update(ref DataSet);
                  idxData++;
                } else {
                  svc.GetaNewUD03(ref DataSet);
                  
                  //DataSet.UD01[idxData]["No"] = idxData + 1;
                  DataSet.UD03[idxData].Company = callContextClient.CurrentCompany;
                  DataSet.UD03[idxData].Key1 = Guid.NewGuid().ToString().ToUpper(); 
                  DataSet.UD03[idxData].Key2 = diNo;
                  DataSet.UD03[idxData].Key5 = "DeliveryInstruction";
                  
                  DataSet.UD03[idxData].Character01 = plantName;
                  
                  DataSet.UD03[idxData].ShortChar01 = gate;
                  DataSet.UD03[idxData].ShortChar02 = poNum;
                  DataSet.UD03[idxData].ShortChar03 = plantID;
                  DataSet.UD03[idxData].ShortChar04 = partNum;
                  DataSet.UD03[idxData].ShortChar05 = uom;
                  DataSet.UD03[idxData].ShortChar06 = dnNo;
                  
                  DataSet.UD03[idxData].Number01 = poItem;
                  DataSet.UD03[idxData].Number02 = qty;
                  DataSet.UD03[idxData].Number03 = Convert.ToInt32(ordNum);
                  DataSet.UD03[idxData].Number04 = Convert.ToInt32(lineNum);
                  
                  DataSet.UD03[idxData].Date01 = diRcvDate;
                  DataSet.UD03[idxData].Character02 = DateTime.Now.ToString("yyyyMMdd HHmmss");
                  
                  if (isExists) {
                    DataSet.UD03[idxData].ShortChar20 = "FAILED it's already been uploaded";
                  } else {
                    if (part == null) {
                      DataSet.UD03[idxData].ShortChar20 = "FAILED Part not found";
                    } else {
                      DataSet.UD03[idxData].ShortChar20 = "FAILED Sales Order not found";
                    }
                  }
                  
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
        CallService<Ice.Contracts.UD03SvcContract>(svc => {
         svc.Update(ref DataSet);
        });
        txScope.Complete();
      }
    }
  }
} catch(Exception ex){
  throw new Ice.BLException($"getDataDI => {lineReader}: {ex.Message}");
}
