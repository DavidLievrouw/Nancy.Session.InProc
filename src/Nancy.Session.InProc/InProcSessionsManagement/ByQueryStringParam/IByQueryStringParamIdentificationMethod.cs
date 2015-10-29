namespace Nancy.Session.InProc.InProcSessionsManagement.ByQueryStringParam {
  internal interface IByQueryStringParamIdentificationMethod : IInProcSessionIdentificationMethod {
    string ParameterName { get; set; }
  }
}