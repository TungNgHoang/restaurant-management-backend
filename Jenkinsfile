/* groovylint-disable LineLength, NoDef, VariableTypeRequired */
def version = "0.${BUILD_NUMBER}"

pipeline {
    agent any

    environment {
        GIT_REPO = 'https://github.com/TungNgHoang/restaurant-management-backend.git'
        GIT_BRANCH = 'main'
        GIT_CREDENTIAL = 'github-pat' // Dùng ID trực tiếp, không gọi hàm credentials ở đây
        IMAGE_NAME = 'restaurant-backend'
        VERSION = "${version}"
        SONAR_PROJECT_KEY = 'restaurant-management-backend'
        SONARQUBE_ENV = 'SonarQube' // Tên cấu hình SonarQube trong Jenkins
    }

    stages {
        stage('Check Source') {
            steps {
                echo '🧾 Cloning repository...'
                git url: "${GIT_REPO}", branch: "${GIT_BRANCH}", credentialsId: "${GIT_CREDENTIAL}"
            }
        }

        stage('SonarQube Analysis') {
            environment {
                PATH = "${env.PATH};%USERPROFILE%\\.dotnet\\tools"
            }
            steps {
                withSonarQubeEnv("${SONARQUBE_ENV}") {
                    withCredentials([string(credentialsId: 'sonar-token', variable: 'SONAR_TOKEN')]) {
                        bat 'dotnet tool install --global dotnet-sonarscanner'
                        bat "dotnet sonarscanner begin /k:\"${SONAR_PROJECT_KEY}\" /d:sonar.host.url=%SONAR_HOST_URL% /d:sonar.login=%SONAR_TOKEN%"
                        bat 'dotnet build'
                        bat "dotnet sonarscanner end /d:sonar.login=%SONAR_TOKEN%"
                    }
                }
            }
        }

        stage('Quality Gate') {
            steps {
                timeout(time: 10, unit: 'MINUTES') {
                    script {
                        def qg = waitForQualityGate()
                        if (qg.status != 'OK') {
                            error("❌ Quality Gate failed: ${qg.status}")
                        } else {
                            echo '✅ Quality Gate passed.'
                        }
                    }
                }
            }
        }

        stage('Build & Push Docker Image') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'gitlab-registry-token', usernameVariable: 'REGISTRY_USER', passwordVariable: 'REGISTRY_PASS')]) {
                    bat 'dotnet publish -c Release -o out'
                    bat "docker login -u %REGISTRY_USER% -p %REGISTRY_PASS% ${env.REGISTRY_URL}"
                    bat "docker build -t ${IMAGE_NAME}:${VERSION} ."
                    bat "docker push ${IMAGE_NAME}:${VERSION}"
                }
            }
        }

        // stage('Deploy on Server') {
        //     steps {
        //         echo '🚀 Deploying new image via SSH'
        //         withCredentials([sshUserPrivateKey(credentialsId: 'your-ssh-key-id', keyFileVariable: 'KEYFILE')]) {
        //             bat """
        //                 powershell -Command \"
        //                     ssh -i '%KEYFILE%' -o StrictHostKeyChecking=no -p 21022 it23@101.99.23.156 \\
        //                         \\\"docker rm -f restaurant-backend || true && \\
        //                         docker run -d --name restaurant-backend -p 5000:80 ${IMAGE_NAME}:${VERSION}\\\"
        //                 \"
        //             """
        //         }
        //     }
        // }
    }
}
