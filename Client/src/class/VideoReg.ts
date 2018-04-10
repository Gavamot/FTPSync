export class VideoReg
{
  constructor(
    public id: number,
    public brigadeCode: number,
    public ip : string,
    public user : string,
    public password : string,
    public channelFolder : string,
    public channelAutoLoad : AutoLoadStatus,
    public channelTimeStamp : string,
    public videoFolder : string
  ) { }
}
