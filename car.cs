


namespace PROGRAM
{

    public class Car
    {
        private string _brand;

        public string Brand
        {
            get{ return _brand; }
            set
            {
                _brand= value.ToUpper();
            }
        }
        private int _model;
        public int Model
        {
            get{ return _model; }
            set
            {
                if (value < 0){
                    throw new Exception("model cant be negative");
                }
                _model = value;
            }
        }

        public Car()
        {
            this.Brand = "";
            this.Model = 0;
        }
        public Car(string Brand, int Model)
        {
            this.Brand = Brand;
            this.Model = Model;
        }


    }


    
}