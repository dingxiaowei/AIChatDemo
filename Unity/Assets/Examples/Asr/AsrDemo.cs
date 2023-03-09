using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;

namespace BaiduSpeech.Examples
{
    /// <summary>测试语音转文本功能</summary>
    public class AsrDemo : MonoBehaviour
    {
        public Text content;
        public Slider volumeSlider;
        public Text volumeText;
        public Text volumeText2;
        public Text stateText;

        private BaiduSpeechManager m_BaiduSpeechManager;

        public string Text = "你好，Hello world, I am RT-Voice!";
        public string Culture = "zh"; //en
        public bool SpeakWhenReady;
        private string uid; //Unique id of the speech

        private void OnEnable()
        {
            // Subscribe event listeners
            Speaker.Instance.OnVoicesReady += voicesReady;
            Speaker.Instance.OnSpeakStart += speakStart;
            Speaker.Instance.OnSpeakComplete += speakComplete;
        }

        private void OnDisable()
        {
            if (Speaker.Instance != null)
            {
                // Unsubscribe event listeners
                Speaker.Instance.OnVoicesReady -= voicesReady;
                Speaker.Instance.OnSpeakStart -= speakStart;
                Speaker.Instance.OnSpeakComplete -= speakComplete;
            }
        }

        public void Speak(string text)
        {
            uid = Speaker.Instance.Speak(text, null, Speaker.Instance.VoiceForCulture(Culture)); //Speak with the first voice matching the given culture
        }

        private void voicesReady()
        {
            Debug.Log($"RT-Voice: {Speaker.Instance.Voices.Count} voices are ready to use!");

            if (SpeakWhenReady) //Speak after the voices are ready
                Speak(Text);
        }

        private void speakStart(Wrapper wrapper)
        {
            if (wrapper.Uid == uid) //Only write the log message if it's "our" speech
                Debug.Log($"RT-Voice: speak started: {wrapper}");
        }

        private void speakComplete(Wrapper wrapper)
        {
            if (wrapper.Uid == uid) //Only write the log message if it's "our" speech
                Debug.Log($"RT-Voice: speak completed: {wrapper}");
        }

        private void Start()
        {
            stateText.text = "初始化语音识别!";

            m_BaiduSpeechManager = FindObjectOfType<BaiduSpeechManager>();
            m_BaiduSpeechManager.AsrInit();//初始化语音识别
            m_BaiduSpeechManager.onSpeechEventListener += OnSpeechEventListener;

            m_BaiduSpeechManager.RequestPermissions(100, AndroidPermission.RECORD_AUDIO);
            //Debug.Log("是否有录音权限："+m_BaiduSpeechManager.CheckPermissions(AndroidPermission.RECORD_AUDIO));
        }

        private void OnDestroy()
        {
            m_BaiduSpeechManager.onSpeechEventListener -= OnSpeechEventListener;
        }

        /// <summary>开始说话</summary>
        public void VoiceStart()
        {
            string data = "{\"accept-audio-data\":false,\"disable-punctuation\":false,\"accept-audio-volume\":true,\"vad.endpoint-timeout\":0,\"pid\":1537}";
            //string data = "{\"accept-audio-data\":false,\"disable-punctuation\":false,\"accept-audio-volume\":true,\"pid\":1537}";
            m_BaiduSpeechManager.VoiceStart(data);

            content.text = null;
        }

        /// <summary>结束说话</summary>
        public void VoiceEnd()
        {
            m_BaiduSpeechManager.VoiceStop();
        }

        IEnumerator GetAnswer(string question)
        {
            UnityWebRequest request = UnityWebRequest.Get("http://116.205.247.142:8084/GetAnswer.ashx?question=" + question);
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                string receiveContent = request.downloadHandler.text.Trim();
                Debug.Log(receiveContent);
                content.text += "答案:" + receiveContent + "\n";
                Speak(receiveContent);
            }
        }


        /// <summary>百度语音识别事件</summary>
        private void OnSpeechEventListener(SpeechEventListenerInfo callbackMessage)
        {
            string state = callbackMessage.state;
            string param = callbackMessage.param;

            // 引擎就绪，可以说话，一般在收到此事件后通过UI通知用户可以说话了
            if (state.Equals(SpeechConstant.CALLBACK_EVENT_ASR_READY))
            {
                stateText.text = "引擎就绪，可以说话!";
                Debug.Log("引擎就绪，可以说话");
            }

            // 识别结果
            if (state.Equals(SpeechConstant.CALLBACK_EVENT_ASR_PARTIAL))
            {
                stateText.text = "识别结果！";

                Debug.Log("state:" + state + "---" + "params:" + param);

                AsrParams asrParams = Serializable.GetAsrParams(param);

                if (asrParams.results_recognition.Length > 0) Debug.Log("results_recognition:" + asrParams.results_recognition[0]);
                //Debug.Log("result_type:" + asrParams.result_type);
                //Debug.Log("best_result:" + asrParams.best_result);
                //Debug.Log("corpus_no:" + asrParams.origin_result.corpus_no);
                //Debug.Log("err_no:" + asrParams.origin_result.err_no);
                //Debug.Log("raf:" + asrParams.origin_result.raf);
                //Debug.Log("sn:" + asrParams.origin_result.sn);
                //if(asrParams.origin_result.result.word.Length>0) Debug.Log("result:" + asrParams.origin_result.result.word[0]);
                //Debug.Log("error:" + asrParams.error);

                content.text = null;

                if (asrParams.results_recognition.Length > 0)
                {
                    for (int i = 0; i < asrParams.results_recognition.Length; i++)
                    {
                        content.text += "问题:" + asrParams.results_recognition[i] + "\n";
                        StartCoroutine(GetAnswer(asrParams.results_recognition[i]));
                    }
                }
            }

            // 当前音量
            if (state.Equals(SpeechConstant.CALLBACK_EVENT_ASR_VOLUME))
            {
                AsrVolume asrVolume = Serializable.GetAsrVolume(param);
                volumeSlider.value = asrVolume.volume_percent;
                volumeText.text = asrVolume.volume_percent.ToString();
                volumeText2.text = asrVolume.volume.ToString();

                Debug.Log("volume:" + asrVolume.volume);
                Debug.Log("volume_percent:" + asrVolume.volume_percent);
            }

            //一句话识别结束
            if (state.Equals(SpeechConstant.CALLBACK_EVENT_ASR_FINISH))
            {
                stateText.text = "一句话识别结束！";
            }

            //一句话识别结束
            if (state.Equals(SpeechConstant.CALLBACK_EVENT_ASR_END))
            {
                stateText.text = "第一句说话结束！";
            }

            //识别结束释放资源
            if (state.Equals(SpeechConstant.CALLBACK_EVENT_ASR_EXIT))
            {
                stateText.text = "识别结束释放资源！";
                volumeSlider.value = 0;
                volumeText.text = "0";
                volumeText2.text = "0";
            }

            //发生错误
            if (state.Equals(SpeechConstant.CALLBACK_EVENT_ASR_ERROR))
            {
                stateText.text = "发生错误！";
                volumeSlider.value = 0;
                volumeText.text = "0";
                volumeText2.text = "0";
            }

            //Debug.Log("state:"+ state+"---"+ "params:"+ param);

        }
    }
}