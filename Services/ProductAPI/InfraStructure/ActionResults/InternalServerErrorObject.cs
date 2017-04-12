using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductAPI.InfraStructure.ActionResults 
{
    public class InternalServerErrorObject : ObjectResult
    {
        public InternalServerErrorObject(object error) : base(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}