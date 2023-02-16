using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Deleeeeet.Model
{
    enum DeleteStatus
    {
        Deleted,
        Failed,
        NotFound,
    }


    internal class TwitterApi
    {

        private readonly CoreTweet.Tokens tokens;


        public TwitterApi(CoreTweet.Tokens tokens)
        {
            this.tokens = tokens;
        }

        public async Task<DeleteStatus> Delete(long id)
        {
            try
            {
                var result = await tokens.Statuses.DestroyAsync(id);
                return DeleteStatus.Deleted;
            }
            catch(CoreTweet.TwitterException ex)
            {
                if (ex.Status == System.Net.HttpStatusCode.NotFound)
                {
                    //削除済み
                    return DeleteStatus.NotFound;
                }
                return DeleteStatus.Failed;
            }
            catch
            {
                return DeleteStatus.Failed;
            }
            
        }

    }
}
