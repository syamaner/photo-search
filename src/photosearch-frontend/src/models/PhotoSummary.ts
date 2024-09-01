

export interface PhotoSummary {
    Id: string;
    Summaries: {
        [key: string]: ModelResponse;
    };
    Latitude: number;
    Longitude: number;
    Address: string;
}

export interface ModelResponse{
     Summary:string;
     Categories: Array<string>;
     DetectedObjects: Array<string>;
}