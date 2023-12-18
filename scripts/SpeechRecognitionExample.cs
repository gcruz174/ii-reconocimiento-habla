using System;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace HuggingFace.API.Examples {
    public class SpeechRecognitionExample : MonoBehaviour {
        [SerializeField] private Button startButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private TextMeshProUGUI text;

        private AudioClip clip;
        private byte[] bytes;
        private bool recording;
        
        [SerializeField] private Raycast raycast;
        
        private GameObject target = null;

        private void Awake()
        {
            raycast.OnClick += ChangeTarget;
        }

        private void Start() {
            startButton.onClick.AddListener(StartRecording);
            stopButton.onClick.AddListener(StopRecording);
            stopButton.interactable = false;
        }

        private void Update() {
            if (recording && Microphone.GetPosition(null) >= clip.samples) {
                StopRecording();
            }
        }

        private void StartRecording() {
            text.color = Color.white;
            text.text = "Recording...";
            startButton.interactable = false;
            stopButton.interactable = true;
            clip = Microphone.Start(null, false, 10, 44100);
            recording = true;
        }

        private void StopRecording() {
            var position = Microphone.GetPosition(null);
            Microphone.End(null);
            var samples = new float[position * clip.channels];
            clip.GetData(samples, 0);
            bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
            recording = false;
            SendRecording();
        }

        private void SendRecording() {
            text.color = Color.yellow;
            text.text = "Sending...";
            stopButton.interactable = false;
            HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
                text.color = Color.white;
                text.text = response;
                startButton.interactable = true;
                if (response.ToLower().Contains("jump"))
                {
                    target.GetComponent<Rigidbody>().AddForce(Vector3.up * 5, ForceMode.Impulse);
                }

                else if (response.ToLower().Contains("flip"))
                {
                    target.GetComponent<Rigidbody>().AddForce(Vector3.up * 5, ForceMode.Impulse);
                    target.GetComponent<Rigidbody>().AddTorque(Vector3.right * 5, ForceMode.Impulse);
                }
            }, error => {
                text.color = Color.red;
                text.text = error;
                startButton.interactable = true;
            });
        }

        private byte[] EncodeAsWAV(float[] samples, int frequency, int channels) {
            using (var memoryStream = new MemoryStream(44 + samples.Length * 2)) {
                using (var writer = new BinaryWriter(memoryStream)) {
                    writer.Write("RIFF".ToCharArray());
                    writer.Write(36 + samples.Length * 2);
                    writer.Write("WAVE".ToCharArray());
                    writer.Write("fmt ".ToCharArray());
                    writer.Write(16);
                    writer.Write((ushort)1);
                    writer.Write((ushort)channels);
                    writer.Write(frequency);
                    writer.Write(frequency * channels * 2);
                    writer.Write((ushort)(channels * 2));
                    writer.Write((ushort)16);
                    writer.Write("data".ToCharArray());
                    writer.Write(samples.Length * 2);

                    foreach (var sample in samples) {
                        writer.Write((short)(sample * short.MaxValue));
                    }
                }
                return memoryStream.ToArray();
            }
        }
        
        private void ChangeTarget(GameObject newTarget) {
            target = newTarget;
        }
    }
}