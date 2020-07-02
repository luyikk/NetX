using Akka.Actor;

namespace EventNext_AkkaNet
{
    public class UserActor : ReceiveActor

    {
        public UserActor()
        {
            Receive<Income>(Income =>
            {
                mAmount += Income.Memory;
                this.Sender.Tell(mAmount);

            });
            Receive<Payout>(Outlay =>
            {
                mAmount -= Outlay.Memory;
                this.Sender.Tell(mAmount);
            });
            Receive<Get>(Outlay =>
            {
                this.Sender.Tell(mAmount);
            });
        }
        private decimal mAmount;



    }

    public class Get
    {

    }


    public class Income
    {

        public decimal Memory { get; set; }
    }

    public class Payout
    {

        public decimal Memory { get; set; }
    }
}
