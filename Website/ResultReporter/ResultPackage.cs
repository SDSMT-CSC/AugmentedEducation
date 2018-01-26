namespace ResultReporter
{
    public class ResultPackage<T>
    {
        #region Members

        private string _Error_Message;
        private T _Return_Value;

        #endregion


        #region Constructor

        public ResultPackage()
        {
            _Error_Message = string.Empty;
            _Return_Value = default(T);
        }

        #endregion


        #region Properties

        public string ErrorMessage
        {
            get { return _Error_Message; }
            set
            {
                _Error_Message = ((string.IsNullOrEmpty(_Error_Message)) 
                    ? value 
                    : $"{_Error_Message}\r\n{value}");
            }
        }

        public T ReturnValue
        {
            get { return _Return_Value; }
            set { _Return_Value = value; }
        }

        #endregion
    }
}
