/* groovylint-disable LineLength, NoDef, VariableTypeRequired */
def version = "0.${BUILD_NUMBER}"

pipeline {
    agent any
    
    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
        timeout(time: 30, unit: 'MINUTES')
        skipStagesAfterUnstable()
    }

    environment {
        GIT_REPO = 'https://github.com/TungNgHoang/restaurant-management-backend.git'
        GIT_BRANCH = 'main'
        GIT_CREDENTIAL = 'github-pat'
        DEPLOY_DIR = 'C:\\inetpub\\wwwroot\\Sinhvien\\pizzadaay\\api'
        VERSION = "${version}"
    }

    stages {
        stage('Check Source') {
            steps {
                echo '🧾 Cloning repository...'
                git url: "${GIT_REPO}", branch: "${GIT_BRANCH}", credentialsId: "${GIT_CREDENTIAL}"
            }
        }

        stage('Backup Current Version') {
            steps {
                script {
                    def backupDir = "${DEPLOY_DIR}_backup_${BUILD_NUMBER}"
                    bat """
                        if exist "${DEPLOY_DIR}" (
                            echo 💾 Creating backup...
                            xcopy /y /s /e /i "${DEPLOY_DIR}" "${backupDir}"
                            echo ✅ Backup created
                        )
                    """
                }
            }
        }

        stage('Build & Test') {
            parallel {
                stage('Build & Publish') {
                    steps {
                        retry(2) {
                            echo '🔨 Building and publishing application...'
                            bat 'dotnet restore --verbosity minimal'
                            bat 'dotnet build -c Release --no-restore'
                            bat 'dotnet publish -c Release -o out --no-build'
                        }
                    }
                }
                stage('Unit Tests') {
                    steps {
                        bat 'dotnet test --no-build --logger trx'
                    }
                    post {
                        always {
                            publishTestResults testResultsPattern: '**/*.trx'
                        }
                    }
                }
            }
        }

        stage('Deploy to IIS') {
            options {
                timeout(time: 5, unit: 'MINUTES')
            }
            steps {
                echo '🚀 Deploying to IIS...'
                bat """
                    echo [%DATE% %TIME%] Stopping IIS application pool...
                    appcmd stop apppool /apppool.name:"DefaultAppPool" || echo "App pool already stopped"
                    
                    echo [%DATE% %TIME%] Copying files...
                    if not exist "${DEPLOY_DIR}" mkdir "${DEPLOY_DIR}"
                    xcopy /y /s /e /i out\\* "${DEPLOY_DIR}\\"
                    
                    echo [%DATE% %TIME%] Starting IIS application pool...
                    appcmd start apppool /apppool.name:"DefaultAppPool"
                """
            }
        }

        stage('Health Check') {
            steps {
                script {
                    echo '🔍 Performing health check...'
                    sleep 20 // Wait for app to start
                    bat """
                        powershell -Command "
                            try {
                                \$response = Invoke-WebRequest -Uri 'http://localhost/api/health' -UseBasicParsing -TimeoutSec 30
                                if (\$response.StatusCode -eq 200) {
                                    Write-Host '✅ Health check passed'
                                } else {
                                    Write-Host '❌ Health check failed'
                                    exit 1
                                }
                            } catch {
                                Write-Host '❌ Health check failed: \$_'
                                exit 1
                            }
                        "
                    """
                }
            }
        }
    }

    post {
        always {
            echo '🧹 Cleaning up workspace...'
            archiveArtifacts artifacts: 'out/**/*', allowEmptyArchive: true
            cleanWs()
        }
        success {
            echo '✅ Pipeline completed successfully!'
            // Add Slack notification here
        }
        failure {
            echo '❌ Pipeline failed!'
            emailext(
            to: 'bit220163@st.cmcu.edu.vn',
            subject: "Build Failed: ${env.JOB_NAME} - ${env.BUILD_NUMBER}",
            body: "Build failed. Check console output at ${env.BUILD_URL}"
        )
            script {
                def backupDir = "${DEPLOY_DIR}_backup_${BUILD_NUMBER}"
                bat """
                    if exist "${backupDir}" (
                        echo 🔄 Rolling back to previous version...
                        rmdir /s /q "${DEPLOY_DIR}" || echo "No deploy dir to remove"
                        xcopy /y /s /e /i "${backupDir}" "${DEPLOY_DIR}" || echo "No backup to restore"
                    )
                """
            }
        }
    }
}
