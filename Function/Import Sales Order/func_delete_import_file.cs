try {
  var folder = Epicor.ServiceModel.Utilities.SpecialFolder.UserData;
  string pattern = @"Import(.+)";
  
  Match match = Regex.Match(pathFile, pattern);
  pathFile = "Import" + match.Groups[1].Value.Trim();
 
  CallService<Ice.Contracts.FileTransferSvcContract>(svc => {
    svc.FileDelete(folder, pathFile);
  });
  
  response = "success";
}
catch(Exception e)
{
  throw new Ice.BLException($"{e.Message}");
}


