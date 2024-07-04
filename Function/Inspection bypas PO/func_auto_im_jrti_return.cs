/**
------------------------------------ ------------------------------
-- Function for Auto Issue Material and Job Receipt to inventory 
-- Create by Tio Choirul 
-- Create date 4 July 2024
------------------------------------ ------------------------------
**/

// Library
//Erp.Contracts.BO.IssueReturn.dll
//Erp.Contracts.BO.ReceiptFromMfg.dll

var rcvDtlRecs = Db.RcvDtl.With(LockHint.NoLock)
  .Where(x => 
    x.Company == this.Session.CompanyID 
    && x.VendorNum == this.Vendor 
    && x.PurPoint == this.purPoint 
    && x.PackSlip == this.PackSlip 
    && x.PackLine == this.PackLine
  ).FirstOrDefault();
  
bool reqUserInput = true;
string msg = "";
string ptmsg = "";
string negQtyTest = "";
bool a = false;
bool negativQty = false;
bool isAktif = true;

string _company = this.CompanyID;
int defJobSeq = 10;
decimal minQty = 0;

string modulID = "RcvDtl";
var ud05Recs = Db.UD05.With(LockHint.NoLock)
  .Where(x => 
    x.Company == this.Session.CompanyID 
    && x.Key1 == this.Vendor.ToString()
    && x.Key2 == this.PackSlip 
    && x.Key3 == this.PackLine.ToString()
    && x.Key5 == modulID
    && x.Number09 != minQty
    && x.CheckBox01 == isAktif
  ).ToList();
  
string defWhs = "J";
string defBin = "1";
decimal serialNo = 0M;

System.Guid pcMtlQueueRowID = new System.Guid();
this.error = "0";

if (rcvDtlRecs != null) {
  
  try {
    // GET DATA JOB ASM FROM RCV DTL
    var jobAsm = Db.JobAsmbl.With(LockHint.NoLock)
      .Where(x => 
        x.Company == _company 
        && x.JobNum == rcvDtlRecs.JobNum 
        && x.AssemblySeq == rcvDtlRecs.AssemblySeq
      ).FirstOrDefault();
      
    // GET DATA JOB MATERIAL FROM RCV DTL
    var jobMtl = Db.JobMtl.With(LockHint.NoLock)
      .Where(x => 
        x.Company == _company 
        && x.JobNum == rcvDtlRecs.JobNum 
        && x.AssemblySeq == rcvDtlRecs.AssemblySeq 
        && x.MtlSeq == defJobSeq
      ).FirstOrDefault();
    
    // GET PART FOR JOB MATERIAL
    var part = Db.Part.With(LockHint.NoLock)
      .Where(x => 
        x.Company == _company 
        && x.PartNum == jobMtl.PartNum
      ).FirstOrDefault();
    
    this.error = "0";
    
    using(var txScope = Erp.ErpContext.CreateDefaultTransactionScope()) {
    
      // *************************************************************
      // Issue Material
      // *************************************************************
      foreach(var rUD05 in ud05Recs) {
        decimal qtyIM = rUD05.Number09;
        
        if (qtyIM != 0) {
          // GET DATA PART TRAN FROM RCV DTL
          var partTran = Db.PartTran.With(LockHint.NoLock)
            .Where(x => 
              x.Company == _company 
              && x.TranNum == rUD05.Number04
              && x.LegalNumber == rUD05.Character02
            ).FirstOrDefault();
            
          //throw new BLException("Return Submit => " + partTran.TranQty);
          this.CallService<Erp.Contracts.IssueReturnSvcContract>(imSvc => {
            var ds = new Erp.Tablesets.IssueReturnTableset();
            
            imSvc.GetNewIssueReturnFromJob(rcvDtlRecs.JobNum, rcvDtlRecs.AssemblySeq, "MTL-STK", Guid.Empty, ref ds);
            var ttIssueReturn_Row = (from IsRetRow in ds.IssueReturn select IsRetRow).FirstOrDefault();
    
            if (ttIssueReturn_Row != null) {
              
              ttIssueReturn_Row.FromWarehouseCode = partTran.WareHouseCode;
              ttIssueReturn_Row.FromBinNum = partTran.BinNum;
              ttIssueReturn_Row.FromAssemblyPartNum = jobAsm.PartNum;
              ttIssueReturn_Row.FromAssemblyPartDescription = jobAsm.Description;
              ttIssueReturn_Row.FromJobSeq = jobMtl.MtlSeq;        
              ttIssueReturn_Row.FromJobSeqPartNum = jobMtl.PartNum;        
              ttIssueReturn_Row.FromJobSeqPartDescription = jobMtl.Description; 
              
              ttIssueReturn_Row.ToWarehouseCode = partTran.WareHouseCode;
              ttIssueReturn_Row.ToBinNum = partTran.BinNum;
              ttIssueReturn_Row.PartNum = jobMtl.PartNum;
            
              
              ttIssueReturn_Row.PartIUM = part.IUM;
              ttIssueReturn_Row.UM = part.IUM;
              ttIssueReturn_Row.DimConvFactor = 1;
                     
              ttIssueReturn_Row.LotNum = partTran.LotNum;
              ttIssueReturn_Row.TranQty = qtyIM;   //rcvDtl.PassedQty;
              ttIssueReturn_Row.TranDate = Convert.ToDateTime(rcvDtlRecs.qcdate_c); //BpmFunc.Today();   //rcvDtl.InspectedDate; 
              ttIssueReturn_Row.PartTrackLots = true;
              
              ttIssueReturn_Row.TranDocTypeID = "RMP";
              ttIssueReturn_Row.TranReference = "Auto Return Material from Receipt PO : " + rcvDtlRecs.PackSlip + "/" + rcvDtlRecs.PackLine + " : " + rUD05.Character02;
              ttIssueReturn_Row.RowMod = "U";
              
              imSvc.MasterInventoryBinTests(ref ds, out negQtyTest, out msg, out msg, out msg, out msg, out msg);
              if(negQtyTest == "Stop") {
                  this.error = "1";
                  throw new BLException("This transaction will result in a negative onhand quantity for the bin.");
              } else {
                this.error = "1";
                imSvc.PrePerformMaterialMovement(ref ds, out reqUserInput);
                if (reqUserInput) throw new BLException("Need user input.");
                imSvc.PerformMaterialMovement2(negativQty, ref ds, out msg, out msg, out msg);
              }
              this.error = "0";
              this.result = "Return Issue Material Success";
            } else {
              this.error = "1";
              throw new BLException("Return Issue Material table is null.");
            }
            
            imSvc.Dispose();
            //ds = null;
          });
        }
      }

      // jika, im berhasil
      if (this.error == "0") {
        // *************************************************************
        // Job Receipt to Inventory
        // *************************************************************
        this.CallService<Erp.Contracts.ReceiptsFromMfgSvcContract>(rcvMfgSvc => {
          var mfgDs = new Erp.Tablesets.ReceiptsFromMfgTableset();
          
          rcvMfgSvc.GetNewReceiptsFromMfg("MFG-STK", ref mfgDs);
          var ttPartTran_Row = (from PartTranRow in mfgDs.PartTran select PartTranRow).FirstOrDefault();
          
          if (ttPartTran_Row != null) {
            ttPartTran_Row.Company = callContextClient.CurrentCompany;
            ttPartTran_Row.Plant2 = callContextClient.CurrentPlant;
            ttPartTran_Row.JobNum = rcvDtlRecs.JobNum;
            ttPartTran_Row.AssemblySeq = rcvDtlRecs.AssemblySeq;
            ttPartTran_Row.PartNum = rcvDtlRecs.PartNum;
            ttPartTran_Row.PartDescription = rcvDtlRecs.PartDescription;
            
            ttPartTran_Row.WareHouseCode = defWhs;
            ttPartTran_Row.BinNum = defBin;
            ttPartTran_Row.LotNum = rcvDtlRecs.LotNum;
            
            ttPartTran_Row.WareHouse2 = "J";
            ttPartTran_Row.BinNum2 = "1";
            ttPartTran_Row.LotNum2 = rcvDtlRecs.LotNum;        
            
            ttPartTran_Row.PartNumTrackLots = true;
            ttPartTran_Row.TranQty = -1*rcvDtlRecs.qcqtypass_c;
            ttPartTran_Row.ThisTranQty = -1*rcvDtlRecs.qcqtypass_c;
            ttPartTran_Row.TranDocTypeID = "RMP";
            ttPartTran_Row.TranReference = "Auto Return JRTI from Receipt PO : " + rcvDtlRecs.PackSlip + "/" + rcvDtlRecs.PackLine;
            
            ttPartTran_Row.TranDate = Convert.ToDateTime(rcvDtlRecs.qcdate_c); // BpmFunc.Today();  //rcvDtl.InspectedDate;
            
            rcvMfgSvc.NegativeInventoryTest(rcvDtlRecs.PartNum, defWhs, defBin, rcvDtlRecs.LotNum, 0, "", "", 0, rcvDtlRecs.qcqtypass_c, out negQtyTest, out msg);
            if(negQtyTest == "Stop") {
              this.error = "1";
              throw new BLException("This transaction will result in a negative onhand quantity for the bin.");
            } else {
              this.error = "1";
              rcvMfgSvc.PreUpdate(ref mfgDs, out reqUserInput);
              if (reqUserInput) throw new BLException("Need user input.");
              rcvMfgSvc.ReceiveMfgPartToInventory(ref mfgDs, serialNo, negativQty, out msg, out msg, msg);
              
              this.error = "0";
              this.result += " & Reverse Job Receipt to Inventory Success";
            }
          } else {
            this.error = "1";
            this.result = "Job Receipt to Inventory Part Tran table is null.";
            throw new BLException("Job Receipt to Inventory Part Tran table is null.");
          }
        
          rcvMfgSvc.Dispose();
          
        });
      }

      // Update Qty IM UD05
      if (this.error == "0") {
        foreach(var rUD05 in ud05Recs) {
          Ice.Tablesets.UD05Tableset DataSet = new Ice.Tablesets.UD05Tableset();   
          this.CallService<Ice.Contracts.UD05SvcContract>(ud05Svc => 
          {
            string whereClauseUD05 = "Key1 = '" + rUD05.Key1 + "' AND Key2 = '"+ rUD05.Key2 +"' AND Key3 = '"+ rUD05.Key3 +"' AND Key4 = '"+ rUD05.Key4 +"' AND Key5 = '"+ rUD05.Key5 +"' " ;
            bool morePage = true;
            DataSet = ud05Svc.GetRows(whereClauseUD05, "", 100, 1, out morePage);
            
            if (DataSet.UD05.Count != 0) 
            {
              foreach(var x in DataSet.UD05) 
              {
                x.Number07 = 0; // Kosongkan Qty IM
                x.Number09 = 0; // Kosongkan, karena udah di return
                x.Number08 = (x.Number08 - rUD05.Number09); // Kurangkan total IM nya?? Aslinya tidak berguna?
                
                x.CheckBox01 = true; // Tanda row ini masih aktif
               
                x.RowMod = "U";
              }
            }
            ud05Svc.Update(ref DataSet);
            
            ud05Svc.Dispose();
          });
        }
      }
      
      txScope.Complete();
    };
    

  } catch (Exception ex) {
    this.error = "1";
    this.result = "Return Submit => Exception: \n" + ex.Message.ToString();
    throw new BLException("Return Submit => " + ex.Message.ToString());
  }
} else {
    this.error = "1";
    this.result = "Return Submit => Tidak ada data";
  throw new BLException("Return Submit => Tidak ada data");
}