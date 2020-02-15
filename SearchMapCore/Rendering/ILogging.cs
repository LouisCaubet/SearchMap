namespace SearchMapCore.Rendering {

    public interface ILogging {

        void Debug(string log);
        void Info(string log);
        void Warning(string log);
        void Error(string log);

    }

}
