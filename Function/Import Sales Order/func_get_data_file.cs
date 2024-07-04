int lineReader = 0;
int idxData = 0;

var serverPath = new Ice.Hosting.ServerDataPath(this.Session);
var rootFolderPath = System.IO.Path.Combine(serverPath.UserDataDirectory, "Uploads", this.path);
bool locked = true;

try{
//throw new Ice.BLException($"{rootFolderPath}");
  using (StreamReader reader = new StreamReader(rootFolderPath))
  {
    string delimiter = "";
    Ice.Tablesets.UD01Tableset DataSet = new Ice.Tablesets.UD01Tableset();
    while (!reader.EndOfStream){
      lineReader++;
      string readData = reader.ReadLine();
      if(lineReader == 1){
        delimiter = "/t";// readData.Substring(4, 1);
      } else {
        string[] dataSplit = Regex.Split(readData, @"\t"); //readData.Split(delimiter);
        if(dataSplit[0] != ""){
          
            CallService<Ice.Contracts.UD01SvcContract>(svc => {
                                
                string poNum = dataSplit[1];
                
                DateTime poDate;
                DateTime.TryParseExact(dataSplit[2], "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out poDate);
                
                int TOP = Convert.ToInt32(dataSplit[8]);
                string plantAHM = dataSplit[10];
                string Curr = dataSplit[11];
                string poItem = dataSplit[16];
                string partNum = dataSplit[17];
                string partDesc = dataSplit[18];
                decimal ActPOQty = Convert.ToDecimal(dataSplit[20]);
                
                DateTime dlvDate;
                DateTime.TryParseExact(dataSplit[21], "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dlvDate);
                
                decimal price = Convert.ToDecimal(dataSplit[22]);
                decimal per = Convert.ToDecimal(dataSplit[23]);
                string orderUOM = dataSplit[24];
                decimal totalPrice = Convert.ToDecimal(dataSplit[28]);
                string gate = dataSplit[29];
                string AHMPartNum = dataSplit[32];
                decimal oriQtyPo = Convert.ToDecimal(dataSplit[33]);
                
                bool isExists = this.ThisLib.checkFromDb(poNum, partNum, poDate, plantAHM, poItem);
                var part = Db.Part.Where(e => e.PartNum == partNum).FirstOrDefault();
                
                //if (lineReader == 55) {
                
                //  throw new Ice.BLException($"{isExists} {part.PartNum}");
                //}
                
                if(!isExists && part != null){
                  svc.GetaNewUD01(ref DataSet);
                  
                  //DataSet.UD01[idxData]["No"] = idxData + 1;
                  DataSet.UD01[idxData].Company = callContextClient.CurrentCompany;
                  DataSet.UD01[idxData].Key1 = Guid.NewGuid().ToString().ToUpper(); 
                  DataSet.UD01[idxData].Key2 = poNum;
                  DataSet.UD01[idxData].Key5 = "SalesOrder";
                  
                  DataSet.UD01[idxData].Date01 = poDate.Date;
                  DataSet.UD01[idxData].Date02 = dlvDate.Date;
                  
                  DataSet.UD01[idxData].Character01 = plantAHM;
                  DataSet.UD01[idxData].Character02 = partNum;
                  DataSet.UD01[idxData].Character03 = part != null ? part.PartDescription : "";
                  DataSet.UD01[idxData].Character04 = AHMPartNum;
                  DataSet.UD01[idxData].Character05 = partDesc;
                  
                  DataSet.UD01[idxData].ShortChar01 = Curr;
                  DataSet.UD01[idxData].ShortChar02 = orderUOM;
                  DataSet.UD01[idxData].ShortChar03 = gate;
                  DataSet.UD01[idxData].ShortChar04 = poItem;
                  
                  DataSet.UD01[idxData].Number01 = TOP;
                  //DataSet.UD01[idxData].Number02 = poItem;
                  DataSet.UD01[idxData].Number03 = ActPOQty;
                  DataSet.UD01[idxData].Number04 = price;
                  DataSet.UD01[idxData].Number05 = per;
                  DataSet.UD01[idxData].Number06 = totalPrice;
                  DataSet.UD01[idxData].Number07 = oriQtyPo;
                  
                  DataSet.UD01[idxData].ShortChar20 = "OK";
                  DataSet.UD01[idxData].Character10 = DateTime.Now.ToString("yyyyMMdd HHmmss");
                    
                  //svc.Update(ref DataSet);
                  idxData++;
                } else {
                  svc.GetaNewUD01(ref DataSet);
                  
                  //DataSet.UD01[idxData]["No"] = idxData + 1;
                  DataSet.UD01[idxData].Company = callContextClient.CurrentCompany;
                  DataSet.UD01[idxData].Key1 = Guid.NewGuid().ToString().ToUpper(); 
                  DataSet.UD01[idxData].Key2 = poNum;
                  DataSet.UD01[idxData].Key5 = "SalesOrder";
                  
                  DataSet.UD01[idxData].Date01 = poDate.Date;
                  DataSet.UD01[idxData].Date02 = dlvDate.Date;
                  
                  DataSet.UD01[idxData].Character01 = plantAHM;
                  DataSet.UD01[idxData].Character02 = partNum;
                  DataSet.UD01[idxData].Character03 = part != null ? part.PartDescription : "";
                  DataSet.UD01[idxData].Character04 = AHMPartNum;
                  DataSet.UD01[idxData].Character05 = partDesc;
                  
                  DataSet.UD01[idxData].ShortChar01 = Curr;
                  DataSet.UD01[idxData].ShortChar02 = orderUOM;
                  DataSet.UD01[idxData].ShortChar03 = gate;
                  DataSet.UD01[idxData].ShortChar04 = poItem;
                  
                  DataSet.UD01[idxData].Number01 = TOP;
                  //DataSet.UD01[idxData].Number02 = poItem;
                  DataSet.UD01[idxData].Number03 = ActPOQty;
                  DataSet.UD01[idxData].Number04 = price;
                  DataSet.UD01[idxData].Number05 = per;
                  DataSet.UD01[idxData].Number06 = totalPrice;
                  DataSet.UD01[idxData].Number07 = oriQtyPo;
                  
                  if (isExists) {
                    DataSet.UD01[idxData].ShortChar20 = "FAILED it's already been uploaded";
                  } else {
                    DataSet.UD01[idxData].ShortChar20 = "FAILED Part not found";
                  }
                  
                  DataSet.UD01[idxData].Character10 = DateTime.Now.ToString("yyyyMMdd HHmmss");
                  
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
        CallService<Ice.Contracts.UD01SvcContract>(svc => {
         svc.Update(ref DataSet);
        });
        txScope.Complete();
      }
    }
  }
} catch(Exception ex){
  throw new Ice.BLException($"getData: {lineReader} -> {ex.Message}");
}
