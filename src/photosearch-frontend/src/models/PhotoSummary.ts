/*

Can you write data code for this json:

{
    "Id": "66b67375b1643a66c12cb32b",
    "Summaries": {
        "llava-phi3": "The image captures a vibrant scene of a live concert. The stage, bathed in the glow of blue and red lights, is set against a backdrop of a large screen displaying an urban landscape with buildings painted in shades of blue and white. \n\nTwo bands are performing on this stage - one on the left side of the stage and another on the right. The band on the left is energetically playing drums while their counterparts on the right are strumming guitars. Both bands have a single member each, who are engrossed in their performance.\n\nThe audience, visible at the bottom of the image, appears to be thoroughly enjoying the concert, with many people capturing the moment on their phones. The overall atmosphere is one of excitement and enjoyment, typical of such live performances."
    },
    "Latitude": 51.503144444444445,
    "Longitude": 0.0033388888888888886,
    "Address": "The O2, Tunnel Avenue, Greenwich Peninsula, Royal Borough of Greenwich, London, Greater London, SE10 0DX, United Kingdom"
}

*/
// as I asked above
export interface PhotoSummary {
    Id: string;
    Summaries: {
        [key: string]: string;
    };
    Latitude: number;
    Longitude: number;
    Address: string;
}
