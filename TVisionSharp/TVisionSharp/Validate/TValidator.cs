namespace TVision
{
    public enum TVTransfer { VtDataSize, VtSetData, VtGetData }

    public class TValidator : TObject
    {
        public ushort Status;
        public ushort Options;

        public const ushort VoFill = 0x0001;
        public const ushort VoTransfer = 0x0002;

        public TValidator() { Status = 0; Options = 0; }
        public virtual void Error() { }
        public virtual bool IsValidInput(string s, bool suppressFill) => true;
        public virtual bool IsValid(string s) => true;
        public virtual ushort Transfer(string s, object buffer, TVTransfer flag) => 0;
        public bool Validate(string s) => IsValid(s);
    }

    public class TPXPictureValidator : TValidator
    {
        private string _pic;
        private int _index, _jndex;

        public TPXPictureValidator(string pic, bool autoFill) { _pic = pic; }

        public override void Error() { }
        public override bool IsValidInput(string s, bool suppressFill) => true;
        public override bool IsValid(string s) => true;
        public TPicResult Picture(string input, bool autoFill) => TPicResult.PrComplete;
    }

    public enum TPicResult
    {
        PrComplete, PrIncomplete, PrEmpty, PrError,
        PrSyntax, PrAmbiguous, PrIncompNoFill
    }

    public class TFilterValidator : TValidator
    {
        private string _validChars;

        public TFilterValidator(string aValidChars) { _validChars = aValidChars; }
        public override void Error() { }
        public override bool IsValidInput(string s, bool suppressFill) => true;
        public override bool IsValid(string s) => true;
    }

    public class TRangeValidator : TFilterValidator
    {
        private int _min, _max;

        public TRangeValidator(int aMin, int aMax) : base(null)
        {
            _min = aMin;
            _max = aMax;
        }

        public override void Error() { }
        public override bool IsValid(string s) => true;
        public override ushort Transfer(string s, object buffer, TVTransfer flag) => 0;
    }

    public class TLookupValidator : TValidator
    {
        public override bool IsValid(string s) => true;
        public virtual bool Lookup(string s) => true;
    }

    public class TStringLookupValidator : TLookupValidator
    {
        public TStringCollection Strings;

        public TStringLookupValidator(TStringCollection aStrings) { Strings = aStrings; }
        public override void Error() { }
        public override bool Lookup(string s) => true;
    }
}
