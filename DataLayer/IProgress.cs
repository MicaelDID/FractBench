namespace DataLayer
{
    public interface IProgress
    {
        void Display(int intTotal, int intPicHeight, bool[] arrFlag);
        void Display(int intTotal, int intPicHeight);
    }
}
